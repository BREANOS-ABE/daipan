//-----------------------------------------------------------------------

// <copyright file="ActivityItemViewModel.cs" company="Breanos GmbH">
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ActivityViewModelInterfaces
{
    public class ActivityItemViewModel : DesignerItemViewModelBase, ISupportDataChanges, IHaveIconInfo, IAmActivity
    {
        public ICommand ShowDataChangeWindowCommand { get; private set; }
        public string IconPath { get; set; }
        private ImageSource _icon;
        public ImageSource Icon
        {
            get
            {
                if (_icon == null)
                {
                    if (!string.IsNullOrEmpty(IconPath))
                    {

                        _icon = ActivityIconGetter.GetOrDefault(IconPath);
                    }
                }
                return _icon;
            }
        }
        public ObservableCollection<SettingInfo> Settings { get; set; }
        public string ActivityName { get; set; }
        public ActivityGuardType SelectedActivityGuardType { get; set; }

        public void ExecuteShowDataChangeWindowCommand(object parameter)
        {
            ActivityItemData data = new ActivityItemData(ActivityName, Id,Settings, SelectedActivityGuardType);
            if (visualiserService.ShowDialog(data) == true)
            {
                this.ActivityName = data.ActivityName;
                this.Id = data.Id;
                this.SelectedActivityGuardType = data.SelectedGuardType;
                this.Settings = data.Settings;
            }
            NotifyChanged(nameof(Settings));
        }
        public ActivityItemViewModel(IDiagramViewModel parent) : base(parent)
        {
            Settings = new ObservableCollection<SettingInfo>();
            visualiserService = ApplicationServicesProvider.Instance.Provider.VisualizerService;
            ShowDataChangeWindowCommand = new SimpleCommand(ExecuteShowDataChangeWindowCommand);
            this.ShowConnectors = false;
        }
        public ActivityItemViewModel(int id, DiagramViewModel parent, double left, double top) : base(id, parent, left, top)
        {
            Settings = new ObservableCollection<SettingInfo>();
            visualiserService = ApplicationServicesProvider.Instance.Provider.VisualizerService;
            ShowDataChangeWindowCommand = new SimpleCommand(ExecuteShowDataChangeWindowCommand);
            this.ShowConnectors = false;
        }
        private IUIVisualizerService visualiserService;
    }

    public class ActivityItemData : INPCBase
    {
        public string ActivityName { get; set; }
        public int Id { get; set; }
        public ObservableCollection<SettingInfo> Settings { get; set; }
        public ActivityGuardType[] GuardTypes => (ActivityGuardType[])Enum.GetValues(typeof(ActivityGuardType));
        public SimpleCommand AddActivitySettingCommand { get; set; }
        public SimpleCommand RemoveActivitySettingCommand { get; set; }
        public ActivityGuardType SelectedGuardType { get; set; }
        public ActivityItemData(string activityName, int id, IEnumerable<SettingInfo> settings, ActivityGuardType selectedActivityGuardType = ActivityGuardType.Or)
        {
            ActivityName = activityName;
            Id = id;
            SelectedGuardType = selectedActivityGuardType;
            Settings = new ObservableCollection<SettingInfo>(settings);
            AddActivitySettingCommand = new SimpleCommand(ExecuteAddActivitySettingCommand);
            RemoveActivitySettingCommand = new SimpleCommand(ExecuteRemoveActivitySettingCommand);
        }
        private void ExecuteAddActivitySettingCommand(object parameter)
        {
            Settings.Add(new SettingInfo());
        }
        private void ExecuteRemoveActivitySettingCommand(object parameter)
        {
            var setting = parameter as SettingInfo;
            if (setting != null)
                Settings.Remove(setting);
        }
    }
    public enum ActivityGuardType
    {
        Or,
        And
    }
    public class SettingInfo
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
