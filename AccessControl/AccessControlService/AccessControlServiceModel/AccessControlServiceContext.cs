//-----------------------------------------------------------------------

// <copyright file="AccessControlServiceContext.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

using System.Configuration;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design;

namespace AccessControlServiceModel
{
    public class AccessControlServiceContext : DbContext, IDisposable
    {
        public const string SettingsPrefix = "AccessControl.ServiceContext";
        public AccessControlServiceContext(string connectionString) : base()
        {
            _connectionString = connectionString;
            //Settings = settings.Where(x => x.Key.StartsWith(SettingsPrefix)).ToDictionary(x => x.Key, x => x.Value);
        }
        public AccessControlServiceContext() : base()
        {

        }

        //public Dictionary<string, string> Settings { get; set; }
        string _connectionString;
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<GroupPermission> GroupPermissions { get; set; }
        public DbSet<KpuMetadata> KpuMetadata { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //var conString = Settings[SettingsPrefix + ".ConnectionString"];
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(_connectionString
                //@"data source=BRE-SQL01\SQL2016;initial catalog=ACSSecurity;integrated security=True;multipleactiveresultsets=True;application name=AccessControlService"
                );
        }
        protected override void OnModelCreating(ModelBuilder m)
        {
            m.Entity<Permission>().HasIndex(u => u.PermissionName).IsUnique();
            m.Entity<GroupPermission>()
                .HasKey(gp => new { gp.GroupIdentification, gp.PermissionId });
            m.Entity<KpuMetadataPermission>()
                .HasKey(kmp => new { kmp.KpuMetadataId, kmp.PermissionId });
            m.Entity<KpuMetadataPermission>()
                .HasOne(kmp => kmp.Permission)
                .WithMany(p => p.KpuMetadataPermission)
                .HasForeignKey(kmp => kmp.PermissionId);
            m.Entity<KpuMetadataPermission>()
                .HasOne(kmp => kmp.KpuMetadata)
                .WithMany(km => km.KpuMetadataPermissions)
                .HasForeignKey(kmp => kmp.KpuMetadataId);
        }
    }
    public class AccessControlServiceContextFactory : IDesignTimeDbContextFactory<AccessControlServiceContext>
    {
        public AccessControlServiceContextFactory(Dictionary<string, string> settings)
        {
            _settings = settings;
        }
        Dictionary<string, string> _settings;
        public AccessControlServiceContextFactory() { }
        public AccessControlServiceContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AccessControlServiceContext>();
            if (_settings == null)
            {
                var conString = @"data source=BRE-SQL01\SQL2016;initial catalog=ACSSecurity;integrated security=True;multipleactiveresultsets=True;application name=AccessControlService";
                optionsBuilder.UseSqlServer(conString);
                return new AccessControlServiceContext(conString);
            }
            else
            {
                var conString =_settings[AccessControlServiceContext.SettingsPrefix + ".ConnectionString"];
                optionsBuilder.UseSqlServer(conString);
                return new AccessControlServiceContext(conString);
            }
        }
    }
}
