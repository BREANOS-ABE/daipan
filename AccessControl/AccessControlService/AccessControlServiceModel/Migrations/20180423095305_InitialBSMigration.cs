//-----------------------------------------------------------------------

// <copyright file="20180423095305_InitialBSMigration.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AccessControlServiceModel.Migrations
{
    public partial class InitialBSMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ACS");

            migrationBuilder.CreateTable(
                name: "Permission",
                schema: "ACS",
                columns: table => new
                {
                    PermissionId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PermissionName = table.Column<string>(maxLength: 128, nullable: false),
                    ParentPermissionId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => x.PermissionId);
                    table.ForeignKey(
                        name: "FK_Permission_Permission_ParentPermissionId",
                        column: x => x.ParentPermissionId,
                        principalSchema: "ACS",
                        principalTable: "Permission",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupPermission",
                schema: "ACS",
                columns: table => new
                {
                    GroupIdentification = table.Column<string>(maxLength: 128, nullable: false),
                    PermissionId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPermission", x => new { x.GroupIdentification, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_GroupPermission_Permission_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "ACS",
                        principalTable: "Permission",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupPermission_PermissionId",
                schema: "ACS",
                table: "GroupPermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_ParentPermissionId",
                schema: "ACS",
                table: "Permission",
                column: "ParentPermissionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupPermission",
                schema: "ACS");

            migrationBuilder.DropTable(
                name: "Permission",
                schema: "ACS");
        }
    }
}
