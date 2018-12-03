//-----------------------------------------------------------------------

// <copyright file="IDatabaseAccessService.cs" company="Breanos GmbH">
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

namespace DesignerTool.Persistence.Common
{
    public interface IDatabaseAccessService
    {
        //delete methods
        void DeleteConnection(int connectionId);
        void DeletePersistDesignerItem(int persistDesignerId);
        void DeleteSettingDesignerItem(int settingsDesignerItemId);

        //save methods
        int SaveDiagram(DiagramItem diagram);
        //PersistDesignerItem is pecific to the DemoApp example
        int SavePersistDesignerItem(PersistDesignerItem persistDesignerItemToSave);
        //SettingsDesignerItem is pecific to the DemoApp example
        int SaveSettingDesignerItem(SettingsDesignerItem settingsDesignerItemToSave);
        int SaveConnection(Connection connectionToSave);

        //Fetch methods
        IEnumerable<DiagramItem> FetchAllDiagram();
        DiagramItem FetchDiagram(int diagramId);
        //PersistDesignerItem is pecific to the DemoApp example
        PersistDesignerItem FetchPersistDesignerItem(int settingsDesignerItemId);
        //SettingsDesignerItem is pecific to the DemoApp example
        SettingsDesignerItem FetchSettingsDesignerItem(int settingsDesignerItemId);
        Connection FetchConnection(int connectionId);
    }
}
