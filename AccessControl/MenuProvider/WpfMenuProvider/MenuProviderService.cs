//-----------------------------------------------------------------------

// <copyright file="MenuProviderService.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using MenuProvider.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WpfMenuProvider
{
    /// <summary>
    /// dummy implementation
    /// 
    /// </summary>
    public class WpfMenuProviderService : IMenuProvider
    {
        public string ConnectionString { get; set; }
        public static WpfMenuProviderService Create(string connectionString)
        {
            var service = new WpfMenuProviderService();
            service.ConnectionString = connectionString;
            return service;
        }
        public async Task<MenuDefinition> CreateMenuDefinitionForPermissionSet(long[] permissionIds)
        {
            // from user id get group -> get permissions & get kpus (distinct)
            List<string> menuPieces = new List<string>();
            using (var context = new AccessControlServiceModel.AccessControlServiceContext(ConnectionString))
            {
                var menuMetadata = context.KpuMetadata
                    .Include(km => km.KpuMetadataPermissions)
                    .Where(
                    km => km.KpuMetadataPermissions.Select(kmp => kmp.PermissionId).Any(pId => permissionIds.Contains(pId)))
                    .Where(km => km.MetadataType == 1);
                menuPieces.AddRange(menuMetadata.Select(m => m.Data));
            }
            List<MenuDefinition> definitions = new List<MenuDefinition>();
            foreach (var piece in menuPieces)
            {
                if (!BreanosConnectors.SerializationHelper.TryDeserialize(piece, out MenuDefinition subMenu))
                {
                    return null;
                }
                definitions.Add(subMenu);
            }
            return Merge(definitions.ToArray());
        }
        private MenuDefinition Merge(MenuDefinition a, MenuDefinition b)
        {
            var groups = new List<MenuGroup>();
            foreach (var groupInA in a.MenuGroups)
            {
                MenuGroup x = null;
                var groupInB = b.MenuGroups.FirstOrDefault(g => g.GroupId == groupInA.GroupId);
                if (groupInB != null)
                {
                    x = Merge(groupInA, groupInB);
                }
                else
                {
                    x = groupInA;
                }
                groups.Add(x);
            }
            return new MenuDefinition()
            {
                MenuGroups = groups.ToArray()
            };
        }
        private MenuDefinition Merge(MenuDefinition[] defs)
        {
            var baseDef = defs[0];
            for (int i = 1; i < defs.Length; i++)
            {
                baseDef = Merge(baseDef, defs[i]);
            }
            return baseDef;
        }
        private MenuGroup Merge(MenuGroup a, MenuGroup b)
        {
            return new MenuGroup()
            {
                GroupId = a.GroupId,
                GroupNameResourceId = a.GroupNameResourceId,
                GroupPriority = a.GroupPriority,
                PositionAnchor = a.PositionAnchor,
                MenuItems = Merge(a.MenuItems, b.MenuItems)
            };
        }
        private MenuItem[] Merge(MenuItem[] baseCollection, params MenuItem[] others)
        {
            var baseList = baseCollection.ToList();
            foreach (var other in others)
            {
                var baseItem = baseCollection.FirstOrDefault(x => x.ItemIdentifier == other.ItemIdentifier);
                if (baseItem == null)
                {
                    baseList.Add(other);
                }
                else
                {
                    if (baseItem.IsLeaf ^ other.IsLeaf)
                    {
                        throw new Exception($"Mismatch of menu item versions to be merged:\n[1]:\t{baseItem.TextResourceId}({baseItem})\n[2]:\t{other.TextResourceId}({other})");
                    }
                    baseItem.Children = Merge(baseItem.Children, other.Children);
                }
            }
            return baseList.ToArray();
        }
        private MenuDefinition[] GetDummyDefinition()
        {
            return new MenuDefinition[]
            {
                new MenuDefinition()
                {
                    MenuGroups=new MenuGroup[]
                    {
                        new MenuGroup()
                        {
                            GroupId="MasterG",
                            GroupNameResourceId="MasterG_Text",
                            GroupPriority=1,
                            PositionAnchor=0,
                            MenuItems=new MenuItem[]
                            {
                                new MenuItem()
                                {
                                    ItemIdentifier="HanoiMaster",
                                    TextResourceId="HanoiMaster_Text",
                                    PermissionIdentifier="Hanoi",
                                    Command="Navigate",
                                    CommandParameter="Hanoi",
                                    IconXamlGeometry="dummyXamlGeometry"
                                }
                            }
                        }
                    }
                },
                new MenuDefinition()
                {
                    MenuGroups= new MenuGroup[]
                    {
                        new MenuGroup()
                        {
                            GroupId="MasterG",
                            GroupNameResourceId="MasterG_Text",
                            GroupPriority=1,
                            PositionAnchor=0,
                            MenuItems=new MenuItem[]
                            {
                                new MenuItem()
                                {
                                    ItemIdentifier="HanoiInfos",
                                    TextResourceId="HanoiInfos_Text",
                                    IconXamlGeometry="dummyXaml2",
                                    PermissionIdentifier="Hanoi",
                                    Children= new MenuItem[]
                                    {
                                        new MenuItem()
                                        {
                                            ItemIdentifier="NumerDisksInfo",
                                            Command="Navigate",
                                            CommandParameter="HanoiNumberDisks",
                                            TextResourceId="NumberDisksInfo_Text",
                                            IconXamlGeometry="dummyXaml3",
                                            PermissionIdentifier="Hanoi"
                                        },
                                        new MenuItem()
                                        {
                                            ItemIdentifier="RunRoundsInfo",
                                            Command="Navigate",
                                            CommandParameter="HanoiRunRounds",
                                            TextResourceId="RunRoundsInfo_Text",
                                            IconXamlGeometry="dummyXaml4",
                                            PermissionIdentifier="Hanoi"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
