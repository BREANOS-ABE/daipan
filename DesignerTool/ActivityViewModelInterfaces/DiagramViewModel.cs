//-----------------------------------------------------------------------

// <copyright file="DiagramViewModel.cs" company="Breanos GmbH">
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
using System.Windows.Controls;

namespace ActivityViewModelInterfaces
{
    public class DiagramViewModel : INPCBase, IDiagramViewModel
    {
        private ObservableCollection<SelectableDesignerItemViewModelBase> items = new ObservableCollection<SelectableDesignerItemViewModelBase>();
        public const string Tab_Design = "tab_Design";
        public const string Tab_Code = "tab_Code";
        private TabItem _selectedTab;

        public TabItem SelectedTab
        {
            get { return _selectedTab; }
            set {
                DeselectAll();
                NotifyChanged("Items");
                _selectedTab = value;
                TabChanged?.Invoke(this); }
        }

        public delegate void OnTabChanged(object sender);
        public event OnTabChanged TabChanged;
        public DiagramViewModel()
        {
            
            _activities = new ObservableCollection<IAmActivity>();
            
            WorkflowSettings = new ObservableCollection<SettingInfo>();
            AddItemCommand = new SimpleCommand(ExecuteAddItemCommand);
            RemoveItemCommand = new SimpleCommand(ExecuteRemoveItemCommand);
            ClearSelectedItemsCommand = new SimpleCommand(ExecuteClearSelectedItemsCommand);
            CreateNewDiagramCommand = new SimpleCommand(ExecuteCreateNewDiagramCommand);
            AddWorkflowSettingCommand = new SimpleCommand(ExecuteAddWorkflowSettingCommand);
            RemoveWorkflowSettingCommand = new SimpleCommand(ExecuteRemoveWorkflowSettingCommand);
            Mediator.Instance.Register(this);
            
            Initialize();
        }

        public void Initialize()
        {
            foreach (var item in Items)
            {
                var npc = item as INPCBase;
                if (npc != null)
                {
                    npc.PropertyChanged += DrawingBoardItemChanged;
                }
            }
            Items.CollectionChanged += Items_CollectionChanged;
        }
        private void DeselectAll()
        {
            foreach (var item in Items)
            {
                item.IsSelected = false;
            }
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyChanged(nameof(Items));
        }

        [MediatorMessageSink("DoneDrawingMessage")]
        public void OnDoneDrawingMessage(bool dummy)
        {
            foreach (var item in Items.OfType<DesignerItemViewModelBase>())
            {
                item.ShowConnectors = false;
            }
        }
        public ObservableCollection<SettingInfo> WorkflowSettings { get; set; }

        private string _workflowName;

        public string WorkflowName
        {
            get { return _workflowName; }
            set { _workflowName = value; NotifyChanged(nameof(WorkflowName)); }
        }
        private int _workflowId;

        public int WorkflowId
        {
            get { return _workflowId; }
            set { _workflowId = value; NotifyChanged(nameof(WorkflowId)); }
        }
        private string _workflowDescription;

        public string WorkflowDescription
        {
            get { return _workflowDescription; }
            set { _workflowDescription = value; NotifyChanged(nameof(WorkflowDescription)); }
        }


        private string _resultWorkflow;

        public string DiagramXml
        {
            get { return _resultWorkflow; }
            set { _resultWorkflow = value;
                NotifyChanged(nameof(DiagramXml));
            }
        }

        public SimpleCommand AddItemCommand { get; private set; }
        public SimpleCommand RemoveItemCommand { get; private set; }
        public SimpleCommand ClearSelectedItemsCommand { get; private set; }
        public SimpleCommand CreateNewDiagramCommand { get; private set; }
        public SimpleCommand AddWorkflowSettingCommand { get; set; }
        public SimpleCommand RemoveWorkflowSettingCommand { get; set; }
        private ObservableCollection<IAmActivity> _activities;
        private ActivityItemViewModel _startActivity;

        public ActivityItemViewModel StartActivity
        {
            get { return _startActivity; }
            set
            {
                _startActivity = value;
                NotifyChanged(nameof(StartActivity), nameof(Items));
            }
        }

        public ObservableCollection<IAmActivity> Activities
        {
            get { return _activities; }
            set { _activities = value; NotifyChanged(nameof(Activities)); }
        }

        public ObservableCollection<SelectableDesignerItemViewModelBase> Items
        {
            get { return items; }
        }

        public List<SelectableDesignerItemViewModelBase> SelectedItems
        {
            get { return Items.Where(x => x.IsSelected).ToList(); }
        }
        public List<SelectableDesignerItemViewModelBase> SelectedConnections
        {
            get
            {
                return Items.OfType<ConnectorViewModel>().Where(x => x.IsSelected).Select(x => x as SelectableDesignerItemViewModelBase).ToList();
            }
        }
        private void ExecuteAddWorkflowSettingCommand(object parameter)
        {
            WorkflowSettings.Add(new SettingInfo());
        }
        private void ExecuteRemoveWorkflowSettingCommand(object parameter)
        {
            var setting = parameter as SettingInfo;
            if (setting != null)
                WorkflowSettings.Remove(setting);
        }
        private void ExecuteAddItemCommand(object parameter)
        {
            if (parameter is SelectableDesignerItemViewModelBase)
            {
                SelectableDesignerItemViewModelBase item = (SelectableDesignerItemViewModelBase)parameter;
                var npc = item as INPCBase;
                if (npc != null)
                {
                    npc.PropertyChanged += DrawingBoardItemChanged;
                }
                var activity = item as ActivityItemViewModel;
                item.Parent = this;
                items.Add(item);
                if (activity != null)
                {
                    Activities.Add(activity);
                }
            }
        }

        private void DrawingBoardItemChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //NotifyChanged("Items");
            if (e.PropertyName == "IsSelected")
            {
                NotifyChanged("SelectedItems");
                NotifyChanged("SelectedConnections");
            }
        }

        private void ExecuteRemoveItemCommand(object parameter)
        {
            if (parameter is SelectableDesignerItemViewModelBase)
            {
                SelectableDesignerItemViewModelBase item = (SelectableDesignerItemViewModelBase)parameter;
                var activity = item as ActivityItemViewModel;
                items.Remove(item);
                if (activity != null)
                {
                    Activities.Remove(activity);
                }
            }
        }

        private void ExecuteClearSelectedItemsCommand(object parameter)
        {
            foreach (SelectableDesignerItemViewModelBase item in Items)
            {
                item.IsSelected = false;
            }
        }

        private void ExecuteCreateNewDiagramCommand(object parameter)
        {
            Items.Clear();
            Activities.Clear();
        }
    }
}
