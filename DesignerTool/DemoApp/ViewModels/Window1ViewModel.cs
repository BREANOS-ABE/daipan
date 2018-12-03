//-----------------------------------------------------------------------

// <copyright file="Window1ViewModel.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using DiagramDesigner;
using System.ComponentModel;
using System.Windows.Data;
using DesignerTool.Persistence.Common;
using System.Threading.Tasks;
using Microsoft.Win32;
using ActivityViewModelInterfaces;
using DesignerTool.Model;
using System.Windows;

namespace DesignerTool
{
    public class DiagramModel
    {

        Dictionary<(int id, string type), DiagramModelItem> Items { get; set; }
        public List<DiagramModelConnector> Connectors { get; set; }
        public DiagramModel()
        {

        }
        public DiagramModel(IEnumerable<DesignerItemViewModelBase> items, IEnumerable<ConnectorViewModel> connectors)
        {
            Items = items.ToDictionary(i => (i.Id, i.GetType().FullName), i => new DiagramModelItem()
            {
                Id = i.Id,
                Top = i.Top,
                Left = i.Left,
                ShowConnectors = i.ShowConnectors,
                ItemType = i.GetType().FullName
            });
            Connectors = connectors.Select(c => new DiagramModelConnector(c, Items)).ToList();
        }
    }
    public class DiagramModelItem
    {
        public int Id { get; set; }
        public double Top { get; set; }
        public double Left { get; set; }
        public bool ShowConnectors { get; set; }
        public string ItemType { get; set; }
        public override string ToString()
        {
            return $"Item {ItemType} {Id} - Top:[{Top}] Left:[{Left}]";
        }
    }
    public class DiagramModelConnector
    {
        public ConnectorOrientation Orientation { get; set; }
        public bool ShowConnectors { get; set; }
        public int DataItemId { get; set; }
        public DiagramModelItem Source { get; set; }
        public DiagramModelItem Target { get; set; }
        public DiagramModelConnector()
        {

        }
        public DiagramModelConnector(ConnectorViewModel cvm, Dictionary<(int id, string type), DiagramModelItem> items)
        {
            var sourceId = cvm.SourceConnectorInfo.DataItem.Id;
            var sourceType = cvm.SourceConnectorInfo.DataItem.GetType().FullName;
            var target = cvm.SinkConnectorInfo as FullyCreatedConnectorInfo;
            var targetId = target.DataItem.Id;
            var targetType = target.DataItem.GetType().FullName;
            Source = items.Where(i => i.Key.id == sourceId && i.Key.type == sourceType).FirstOrDefault().Value;
            Target = items.Where(i => i.Key.id == targetId && i.Key.type == targetType).FirstOrDefault().Value;
        }
        public override string ToString()
        {
            if (Source != null && Target != null)
                return $"Connector {Source} --> {Target}";
            else if (Source != null)
            {
                return $"Dangling-End Connector from {Source}";
            }
            else if (Target != null)
            {
                return $"Dangling-Start Connector towards {Target}";
            }
            else
            {
                return "Faulty Connector without Source nor Target";
            }
        }
    }
    public class Window1ViewModel : INPCBase
    {

        private List<int> savedDiagrams = new List<int>();
        private int? savedDiagramId;
        private List<SelectableDesignerItemViewModelBase> itemsToRemove;
        private IMessageBoxService messageBoxService;

        private bool isBusy = false;
        private string _historicalDrawingBoardDefinition;

        private DiagramViewModel _diagramViewModel;
        public DiagramViewModel DiagramViewModel
        {
            get
            {
                return _diagramViewModel;
            }
            set
            {
                if (_diagramViewModel != value)
                {
                    if (_diagramViewModel != null)
                    {
                        _diagramViewModel.PropertyChanged -= DiagramViewModel_PropertyChanged;
                        _diagramViewModel.TabChanged -= DiagramViewModel_TabChanged;
                    }
                    _diagramViewModel = value;
                    _diagramViewModel.PropertyChanged += DiagramViewModel_PropertyChanged;
                    _diagramViewModel.TabChanged += DiagramViewModel_TabChanged;
                    NotifyChanged("DiagramViewModel");
                }
            }
        }
        private string _windowTitle;
        private const string __program_Title_Prefix = "DesignerTool";
        public string WindowTitle
        {
            get { return _windowTitle; }
            set { _windowTitle = value; NotifyChanged(nameof(WindowTitle)); }
        }

        public Window1ViewModel()
        {
            WindowTitle = __program_Title_Prefix;
            messageBoxService = ApplicationServicesProvider.Instance.Provider.MessageBoxService;
            ToolBoxViewModel = new ToolBoxViewModel();
            DiagramViewModel = new DiagramViewModel();

            DeleteSelectedItemsCommand = new SimpleCommand(ExecuteDeleteSelectedItemsCommand);
            CreateNewDiagramCommand = new SimpleCommand(ExecuteCreateNewDiagramCommand);
            SaveDiagramCommand = new SimpleCommand(ExecuteSaveDiagramCommand);
            LoadDiagramCommand = new SimpleCommand(ExecuteLoadDiagramCommand);
            ExportToXmlCommand = new SimpleCommand(ExecuteExportToXmlCommand);

            //OrthogonalPathFinder is a pretty bad attempt at finding path points, it just shows you, you can swap this out with relative
            //ease if you wish just create a new IPathFinder class and pass it in right here
            ConnectorViewModel.PathFinder = new OrthogonalPathFinder();

        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateCodeTab();
        }
        private void UpdateCodeTab()
        {
            if (DiagramViewModel.SelectedTab == null || DiagramViewModel.SelectedTab.Name == DiagramViewModel.Tab_Design)
                DiagramViewModel.DiagramXml = WriteDrawingboardDefinition();
        }
        private void UpdateDiagramTab()
        {
            LoadDrawingboardDefinition(DiagramViewModel.DiagramXml, false);
        }
        public SimpleCommand DeleteSelectedItemsCommand { get; private set; }
        public SimpleCommand CreateNewDiagramCommand { get; private set; }
        public SimpleCommand SaveDiagramCommand { get; private set; }
        public SimpleCommand LoadDiagramCommand { get; private set; }
        public SimpleCommand ExportToXmlCommand { get; private set; }
        public ToolBoxViewModel ToolBoxViewModel { get; private set; }



        public bool IsBusy
        {
            get
            {
                return isBusy;
            }
            set
            {
                if (isBusy != value)
                {
                    isBusy = value;
                    NotifyChanged("IsBusy");
                }
            }
        }


        public List<int> SavedDiagrams
        {
            get
            {
                return savedDiagrams;
            }
            set
            {
                if (savedDiagrams != value)
                {
                    savedDiagrams = value;
                    NotifyChanged("SavedDiagrams");
                }
            }
        }

        public int? SavedDiagramId
        {
            get
            {
                return savedDiagramId;
            }
            set
            {
                if (savedDiagramId != value)
                {
                    savedDiagramId = value;
                    NotifyChanged("SavedDiagramId");
                }
            }
        }



        private void ExecuteDeleteSelectedItemsCommand(object parameter)
        {
            itemsToRemove = DiagramViewModel.SelectedItems;
            List<SelectableDesignerItemViewModelBase> connectionsToAlsoRemove = new List<SelectableDesignerItemViewModelBase>();

            foreach (var connector in DiagramViewModel.Items.OfType<ConnectorViewModel>())
            {
                if (ItemsToDeleteHasConnector(itemsToRemove, connector.SourceConnectorInfo))
                {
                    connectionsToAlsoRemove.Add(connector);
                }

                if (!(connector.SinkConnectorInfo is PartCreatedConnectionInfo)&& ItemsToDeleteHasConnector(itemsToRemove, (FullyCreatedConnectorInfo)connector.SinkConnectorInfo))
                {
                    connectionsToAlsoRemove.Add(connector);
                }

            }
            itemsToRemove.AddRange(connectionsToAlsoRemove);
            foreach (var selectedItem in itemsToRemove)
            {
                DiagramViewModel.RemoveItemCommand.Execute(selectedItem);
            }
        }
        private bool IsCurrentDrawingBoardOkToScrap()
        {
            var currentDrawingboard = SerializeDrawingboard();
            var ddef = GetDrawingboardDefinition();
            bool isOkToScrap = false;

            if (_historicalDrawingBoardDefinition == null && (ddef.ActivityItems == null || ddef.ActivityItems.Length <= 0))
            {
                isOkToScrap = true;
            }
            else if (!string.IsNullOrEmpty(_historicalDrawingBoardDefinition) && _historicalDrawingBoardDefinition.Equals(currentDrawingboard))
            {
                isOkToScrap = true;
            }
            return isOkToScrap;
        }
        private bool SaveIfCurrentDrawingBoardDirty()
        {
            var isOkToScrap = IsCurrentDrawingBoardOkToScrap();
            if (!isOkToScrap)
            {
                var mbresult = MessageBox.Show("Do you want to save the current changes to the drawing board?", "Changes found", MessageBoxButton.YesNoCancel);
                if (mbresult == MessageBoxResult.Yes)
                {
                    ExecuteSaveDiagramCommand(null);
                }
                else if (mbresult == MessageBoxResult.Cancel)
                {
                    return false;
                }
            }
            return true;
        }
        private void ExecuteCreateNewDiagramCommand(object parameter)
        {
            if (!SaveIfCurrentDrawingBoardDirty()) return;
            itemsToRemove = new List<SelectableDesignerItemViewModelBase>();
            SavedDiagramId = null;
            CurrentFilename = null;
            DiagramViewModel.CreateNewDiagramCommand.Execute(null);
            _historicalDrawingBoardDefinition = null;
            DesignerItemViewModelBase.ResetCurrentId(1);
            ConnectorViewModel.ResetConnectorId(1);
            DiagramViewModel.DiagramXml = WriteDrawingboardDefinition(); // Update Code Tab doesn't work while on the code tab
        }
        private string SerializeDrawingboard()
        {
            var ddef = new Model.DrawingboardDefinition(DiagramViewModel);
            var sdef = BreanosConnectors.SerializationHelper.Serialize(ddef);
            return sdef;
        }
        private DrawingboardDefinition GetDrawingboardDefinition()
        {
            return new Model.DrawingboardDefinition(DiagramViewModel);
        }
        private const string __DrawingboardDefinitionFileFilter = "Drawingboard definition (*.xdf)|*.xdf";
        private string _currentFilename;

        public string CurrentFilename
        {
            get { return _currentFilename; }
            set { _currentFilename = value; }
        }

        private void ExecuteSaveDiagramCommand(object parameter)
        {
            if (!DiagramViewModel.Items.Any())
            {
                messageBoxService.ShowError("There must be at least one item in order save a diagram");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = __DrawingboardDefinitionFileFilter;
            sfd.FileName = CurrentFilename ?? "";
            if (sfd.ShowDialog() ?? false)
            {
                _historicalDrawingBoardDefinition = SerializeDrawingboard();
                var filename = sfd.FileName;
                System.IO.File.WriteAllText(filename, _historicalDrawingBoardDefinition);
                CurrentFilename = sfd.FileName;
                UpdateTitle(CurrentFilename);
            }
        }
        private string WriteDrawingboardDefinition()
        {
            var ddef = new Model.DrawingboardDefinition(DiagramViewModel);
            var dxml = BreanosConnectors.SerializationHelper.Serialize(ddef);
            return dxml;
        }
        private string WriteWorkflowDefinition()
        {
            var ddef = new Model.DrawingboardDefinition(DiagramViewModel);
            var wfdef = ddef.Workflow;
            return BreanosConnectors.SerializationHelper.Serialize(wfdef);
        }
        private void ScrapCurrentDiagram()
        {
            DiagramViewModel.CreateNewDiagramCommand.Execute(null);

        }
        private void DiagramViewModel_TabChanged(object sender)
        {
            if (DiagramViewModel.SelectedTab.Name == "tab_Design")
            {
                UpdateDiagramTab();
            }
        }

        private bool LoadDrawingboardDefinition(string drawingboardDefinition, bool isSetHistoricalDrawingboardDefinition)
        {
            try
            {
                if (BreanosConnectors.SerializationHelper.TryDeserialize(drawingboardDefinition, out Model.DrawingboardDefinition d))
                {
                    ScrapCurrentDiagram();
                    var diagramViewModel = DiagramViewModel;
                    var vm = d.ToVM(diagramViewModel);
                    diagramViewModel.Activities = new System.Collections.ObjectModel.ObservableCollection<IAmActivity>(/*vm.items.Select(di => di as IAmActivity)*/);
                    var startTransition = d.Workflow.Transitions.FirstOrDefault(t => t.SourceActivityId == -1);
                    if (startTransition != null)
                    {
                        diagramViewModel.StartActivity = vm.items.FirstOrDefault(di => di.Id == startTransition.TargetActivityId) as ActivityItemViewModel;
                    }
                    foreach (var item in vm.items)
                    {
                        diagramViewModel.AddItemCommand.Execute(item);
                    }
                    var connectors = vm.connectors;
                    foreach (var item in connectors)
                    {
                        var srcItem = GetConnectorDataItem(diagramViewModel, item.SourceConnectorInfo.DataItem.Id, item.SourceConnectorInfo.DataItem.GetType());
                        var tgtItem = GetConnectorDataItem(diagramViewModel, (item.SinkConnectorInfo as FullyCreatedConnectorInfo).DataItem.Id, (item.SinkConnectorInfo as FullyCreatedConnectorInfo).DataItem.GetType());
                        var cvm = new ConnectorViewModel(item.Id, diagramViewModel,
                        GetFullConnectorInfo(item.Id, srcItem, item.SourceConnectorInfo.Orientation),
                        GetFullConnectorInfo(item.Id, tgtItem, item.SinkConnectorInfo.Orientation))
                        {
                            ConditionText = item.ConditionText
                        };
                        diagramViewModel.AddItemCommand.Execute(cvm);

                    }
                    if (diagramViewModel?.Items?.Count > 0)
                    {
                        if (diagramViewModel.Items.Any(i => i is ActivityItemViewModel))
                            DesignerItemViewModelBase.ResetCurrentId(diagramViewModel.Items.Where(item => item is ActivityItemViewModel).Max(it => it.Id) + 1);
                        else
                            DesignerItemViewModelBase.ResetCurrentId(1);
                        if (diagramViewModel.Items.Any(i => i is ConnectorViewModel))
                            ConnectorViewModel.ResetConnectorId(diagramViewModel.Items.Where(item => item is ConnectorViewModel).Max(it => it.Id) + 1);
                        else
                            ConnectorViewModel.ResetConnectorId(1);
                    }
                    else
                    {
                        DesignerItemViewModelBase.ResetCurrentId(1);
                        ConnectorViewModel.ResetConnectorId(1);
                    }
                    DiagramViewModel.DiagramXml = WriteDrawingboardDefinition(); // Update Code Tab doesn't work while on the code tab
                }
                if (isSetHistoricalDrawingboardDefinition)
                    _historicalDrawingBoardDefinition = SerializeDrawingboard();
                return true;
            }
            catch (Exception e)
            {

                MessageBox.Show(e.ToString());
                MessageBox.Show($"The used file format isn't supported. Make sure to use a valid Drawingboard Definition file (*.xdf)");
                return false;
            }
        }
        private void ExecuteExportToXmlCommand(object parameter)
        {
            if (!DiagramViewModel.Items.Any())
            {
                messageBoxService.ShowError("There must be at least one item in order save a diagram");
                return;
            }
            var wfXml = WriteWorkflowDefinition();
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Workflow definition (*.xml)|*.xml";
            if (sfd.ShowDialog() ?? false)
            {
                var filename = sfd.FileName;
                System.IO.File.WriteAllText(filename, wfXml);
            }
        }
        private void ExecuteLoadDiagramCommand(object parameter)
        {
            if (!SaveIfCurrentDrawingBoardDirty()) return;

            var ofd = new OpenFileDialog();
            ofd.Filter = __DrawingboardDefinitionFileFilter;
            string filename = null;
            if (ofd.ShowDialog() ?? false)
            {
                filename = ofd.FileName;
            }
            if (string.IsNullOrEmpty(filename)) return;
            var file = System.IO.File.ReadAllText(filename);
            if (LoadDrawingboardDefinition(file, true))
            {
                UpdateTitle(filename);
                CurrentFilename = ofd.FileName;
            }
        }
        private void UpdateTitle(string titlesuffix)
        {
            WindowTitle = __program_Title_Prefix + " - " + titlesuffix;
        }
        private void DiagramViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Items":
                    UpdateCodeTab();
                    break;
                default:
                    break;
            }
        }

        private FullyCreatedConnectorInfo GetFullConnectorInfo(int connectorId, DesignerItemViewModelBase dataItem, ConnectorOrientation connectorOrientation)
        {
            switch (connectorOrientation)
            {
                case ConnectorOrientation.Top:
                    return dataItem.TopConnector;
                case ConnectorOrientation.Left:
                    return dataItem.LeftConnector;
                case ConnectorOrientation.Right:
                    return dataItem.RightConnector;
                case ConnectorOrientation.Bottom:
                    return dataItem.BottomConnector;

                default:
                    throw new InvalidOperationException(
                        string.Format("Found invalid persisted Connector Orientation for Connector Id: {0}", connectorId));
            }
        }


        private DesignerItemViewModelBase GetConnectorDataItem(DiagramViewModel diagramViewModel, int conectorDataItemId, Type connectorDataItemType)
        {
            return diagramViewModel.Items.Where(x => !(x is ConnectorViewModel)).SingleOrDefault(x => x.Id == conectorDataItemId) as DesignerItemViewModelBase;
            //if (connectorDataItemType == typeof(PersistDesignerItemViewModel))
            //{
            //    dataItem = diagramViewModel.Items.OfType<PersistDesignerItemViewModel>().Single(x => x.Id == conectorDataItemId);
            //}

            //if (connectorDataItemType == typeof(SettingsDesignerItemViewModel))
            //{
            //    dataItem = diagramViewModel.Items.OfType<SettingsDesignerItemViewModel>().Single(x => x.Id == conectorDataItemId);
            //}
        }


        private Orientation GetOrientationFromConnector(ConnectorOrientation connectorOrientation)
        {
            Orientation result = Orientation.None;
            switch (connectorOrientation)
            {
                case ConnectorOrientation.Bottom:
                    result = Orientation.Bottom;
                    break;
                case ConnectorOrientation.Left:
                    result = Orientation.Left;
                    break;
                case ConnectorOrientation.Top:
                    result = Orientation.Top;
                    break;
                case ConnectorOrientation.Right:
                    result = Orientation.Right;
                    break;
            }
            return result;
        }


        private ConnectorOrientation GetOrientationForConnector(Orientation persistedOrientation)
        {
            ConnectorOrientation result = ConnectorOrientation.None;
            switch (persistedOrientation)
            {
                case Orientation.Bottom:
                    result = ConnectorOrientation.Bottom;
                    break;
                case Orientation.Left:
                    result = ConnectorOrientation.Left;
                    break;
                case Orientation.Top:
                    result = ConnectorOrientation.Top;
                    break;
                case Orientation.Right:
                    result = ConnectorOrientation.Right;
                    break;
            }
            return result;
        }

        private bool ItemsToDeleteHasConnector(List<SelectableDesignerItemViewModelBase> itemsToRemove, FullyCreatedConnectorInfo connector)
        {
            return itemsToRemove.Contains(connector.DataItem);
        }



        //private void DeleteFromDatabase(DiagramItem wholeDiagramToAdjust, SelectableDesignerItemViewModelBase itemToDelete)
        //{

        //    //make sure the item is removes from Diagram as well as removing them as individual items from database
        //    if (itemToDelete is PersistDesignerItemViewModel)
        //    {
        //        DiagramItemData diagramItemToRemoveFromParent = wholeDiagramToAdjust.DesignerItems.Where(x => x.ItemId == itemToDelete.Id && x.ItemType == typeof(PersistDesignerItem)).Single();
        //        wholeDiagramToAdjust.DesignerItems.Remove(diagramItemToRemoveFromParent);
        //        databaseAccessService.DeletePersistDesignerItem(itemToDelete.Id);
        //    }
        //    if (itemToDelete is SettingsDesignerItemViewModel)
        //    {
        //        DiagramItemData diagramItemToRemoveFromParent = wholeDiagramToAdjust.DesignerItems.Where(x => x.ItemId == itemToDelete.Id && x.ItemType == typeof(SettingsDesignerItem)).Single();
        //        wholeDiagramToAdjust.DesignerItems.Remove(diagramItemToRemoveFromParent);
        //        databaseAccessService.DeleteSettingDesignerItem(itemToDelete.Id);
        //    }
        //    if (itemToDelete is ConnectorViewModel)
        //    {
        //        wholeDiagramToAdjust.ConnectionIds.Remove(itemToDelete.Id);
        //        databaseAccessService.DeleteConnection(itemToDelete.Id);
        //    }

        //    databaseAccessService.SaveDiagram(wholeDiagramToAdjust);


        //}

    }
}
