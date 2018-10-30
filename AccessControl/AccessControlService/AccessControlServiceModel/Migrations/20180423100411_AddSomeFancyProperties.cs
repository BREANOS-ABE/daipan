//-----------------------------------------------------------------------

// <copyright file="20180423100411_AddSomeFancyProperties.cs" company="Breanos GmbH">
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
using Microsoft.EntityFrameworkCore.Migrations;

namespace AccessControlServiceModel.Migrations
{
    public partial class AddSomeFancyProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "GrantedAt",
                schema: "ACS",
                table: "GroupPermission",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Grantee",
                schema: "ACS",
                table: "GroupPermission",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGranted",
                schema: "ACS",
                table: "GroupPermission",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GrantedAt",
                schema: "ACS",
                table: "GroupPermission");

            migrationBuilder.DropColumn(
                name: "Grantee",
                schema: "ACS",
                table: "GroupPermission");

            migrationBuilder.DropColumn(
                name: "IsGranted",
                schema: "ACS",
                table: "GroupPermission");
        }
    }
}
