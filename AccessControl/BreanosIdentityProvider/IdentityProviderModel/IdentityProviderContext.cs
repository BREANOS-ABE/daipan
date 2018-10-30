//-----------------------------------------------------------------------

// <copyright file="IdentityProviderContext.cs" company="Breanos GmbH">
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

namespace IdentityProviderModel
{
    public class IdentityProviderContext : DbContext, IDisposable
    {        
        public string ConnectionString { get; set; }
        public static IdentityProviderContext Create(string connectionString)
        {
            return new IdentityProviderContext
            {
                ConnectionString = connectionString
            };
        }
        
        public IdentityProviderContext() : base() {  }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<UserGroup> UserGroups { get; set; }
        public virtual DbSet<LoginAttempt> LoginAttempts { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(
                ConnectionString
                );
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserIdentifier).IsUnique();
            modelBuilder.Entity<Group>()
                .HasIndex(g => g.GroupIdentifier).IsUnique();
            modelBuilder.Entity<UserGroup>()
                .HasKey(ug => new { ug.UserId, ug.GroupId });
        }
    }
}
