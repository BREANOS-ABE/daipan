//-----------------------------------------------------------------------

// <copyright file="20181022121846_AddKpuMetadataTable.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AccessControlServiceModel.Migrations
{
    public partial class AddKpuMetadataTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "KPU");

            migrationBuilder.CreateTable(
                name: "Metadata",
                schema: "KPU",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 128, nullable: true),
                    Data = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metadata", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MetadataPermission",
                schema: "KPU",
                columns: table => new
                {
                    PermissionId = table.Column<long>(nullable: false),
                    KpuMetadataId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetadataPermission", x => new { x.KpuMetadataId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_MetadataPermission_Metadata_KpuMetadataId",
                        column: x => x.KpuMetadataId,
                        principalSchema: "KPU",
                        principalTable: "Metadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MetadataPermission_Permission_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "ACS",
                        principalTable: "Permission",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MetadataPermission_PermissionId",
                schema: "KPU",
                table: "MetadataPermission",
                column: "PermissionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetadataPermission",
                schema: "KPU");

            migrationBuilder.DropTable(
                name: "Metadata",
                schema: "KPU");
        }
    }
}
