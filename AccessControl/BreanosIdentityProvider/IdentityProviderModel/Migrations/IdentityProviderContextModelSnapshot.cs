﻿//-----------------------------------------------------------------------

// <copyright file="IdentityProviderContextModelSnapshot.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

// <auto-generated />
using System;
using IdentityProviderModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IdentityProviderModel.Migrations
{
    [DbContext(typeof(IdentityProviderContext))]
    partial class IdentityProviderContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.0-preview2-30571")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("IdentityProviderModel.Group", b =>
                {
                    b.Property<long>("GroupId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("GroupIdentifier")
                        .IsRequired()
                        .HasMaxLength(128);

                    b.Property<long?>("ParentGroupId");

                    b.HasKey("GroupId");

                    b.HasIndex("GroupIdentifier")
                        .IsUnique();

                    b.HasIndex("ParentGroupId");

                    b.ToTable("Group","BIP");
                });

            modelBuilder.Entity("IdentityProviderModel.LoginAttempt", b =>
                {
                    b.Property<long>("LoginAttemptId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("LoginDate");

                    b.Property<string>("LoginLocation")
                        .HasMaxLength(128);

                    b.Property<int>("Result");

                    b.Property<long>("UserId");

                    b.HasKey("LoginAttemptId");

                    b.HasIndex("UserId");

                    b.ToTable("LoginAttempt","BIP");
                });

            modelBuilder.Entity("IdentityProviderModel.User", b =>
                {
                    b.Property<long>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedAt");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(128);

                    b.Property<string>("Domain")
                        .HasMaxLength(128);

                    b.Property<DateTime>("FirstLogin");

                    b.Property<string>("FirstName")
                        .HasMaxLength(128);

                    b.Property<DateTime>("LastLogin");

                    b.Property<DateTime>("LastModifiedAt");

                    b.Property<string>("LastModifiedBy");

                    b.Property<string>("LastName")
                        .HasMaxLength(128);

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(48);

                    b.Property<string>("SID")
                        .HasMaxLength(128);

                    b.Property<string>("Salt")
                        .IsRequired()
                        .HasMaxLength(16);

                    b.Property<string>("UserIdentifier")
                        .IsRequired()
                        .HasMaxLength(128);

                    b.HasKey("UserId");

                    b.HasIndex("UserIdentifier")
                        .IsUnique();

                    b.ToTable("User","BIP");
                });

            modelBuilder.Entity("IdentityProviderModel.UserGroup", b =>
                {
                    b.Property<long>("UserId");

                    b.Property<long>("GroupId");

                    b.Property<DateTime>("AddedAt");

                    b.Property<string>("AddedBy")
                        .IsRequired();

                    b.HasKey("UserId", "GroupId");

                    b.HasIndex("GroupId");

                    b.ToTable("UserGroup","BIP");
                });

            modelBuilder.Entity("IdentityProviderModel.Group", b =>
                {
                    b.HasOne("IdentityProviderModel.Group", "ParentGroup")
                        .WithMany("Children")
                        .HasForeignKey("ParentGroupId");
                });

            modelBuilder.Entity("IdentityProviderModel.LoginAttempt", b =>
                {
                    b.HasOne("IdentityProviderModel.User", "User")
                        .WithMany("LoginAttempts")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("IdentityProviderModel.UserGroup", b =>
                {
                    b.HasOne("IdentityProviderModel.Group", "Group")
                        .WithMany("Users")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("IdentityProviderModel.User", "User")
                        .WithMany("Groups")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
