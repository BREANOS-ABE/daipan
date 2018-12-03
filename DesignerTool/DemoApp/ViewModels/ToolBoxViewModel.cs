//-----------------------------------------------------------------------

// <copyright file="ToolBoxViewModel.cs" company="Breanos GmbH">
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
using ActivityViewModelInterfaces;
using DesignerTool.Model;

namespace DesignerTool
{
    public class ToolBoxViewModel
    {
        private List<ToolBoxData> toolBoxItems = new List<ToolBoxData>();

        public ToolBoxViewModel()
        {
            LoadItemsFromActivities();
        }
        private void LoadItemsFromActivities()
        {
            var defs = System.IO.File.ReadAllText($"{ ActivityIconGetter.ImageFolder}knownActivities.xml");
            BreanosConnectors.SerializationHelper.TryDeserialize(defs, out ToolboxDefinition tbd);
            foreach (var item in tbd.Items)
            {
                AddActivityItem(item.ActivityName, item.IconFile);
            }
        }
        private void AddActivityItem(string activityName, string iconPath)
        {
            toolBoxItems.Add(new ToolBoxData(iconPath, activityName, typeof(ActivityItemViewModel)));
        }
        public List<ToolBoxData> ToolBoxItems
        {
            get { return toolBoxItems; }
        }
    }
}
