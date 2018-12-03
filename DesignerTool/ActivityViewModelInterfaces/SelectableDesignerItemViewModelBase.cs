//-----------------------------------------------------------------------

// <copyright file="SelectableDesignerItemViewModelBase.cs" company="Breanos GmbH">
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
using System.Windows.Input;
using System.Xml.Serialization;

namespace ActivityViewModelInterfaces
{

    public interface ISelectItems
    {
        SimpleCommand SelectItemCommand { get;  }
    }


    public abstract class SelectableDesignerItemViewModelBase : INPCBase, ISelectItems
    {
        private bool isSelected;

        public SelectableDesignerItemViewModelBase(int id, IDiagramViewModel parent)
        {
            this.Id = id;
            this.Parent = parent;
            Init();
        }


        public List<SelectableDesignerItemViewModelBase> SelectedItems
        {
            get { return Parent.SelectedItems; }
        }
        public IDiagramViewModel Parent { get; set; }
        public SimpleCommand SelectItemCommand { get; private set; }
        public int Id { get; set; }
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                if (isSelected != value)
                {
                    
                    isSelected = value;
                    NotifyChanged("IsSelected");
                }
            }
        }

        private void ExecuteSelectItemCommand(object param)
        {
            SelectItem((bool)param, !IsSelected);
        }
        
        private void SelectItem(bool newselect, bool select)
        {
            if (newselect)
            {
                foreach (var designerItemViewModelBase in Parent.SelectedItems.ToList())
                {
                    designerItemViewModelBase.isSelected = false;
                }
            }

            IsSelected = select;
        }
    
        private void Init()
        {
            SelectItemCommand = new SimpleCommand(ExecuteSelectItemCommand);
        }
    }
}
