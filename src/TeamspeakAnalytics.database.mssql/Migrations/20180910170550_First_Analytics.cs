﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TeamspeakAnalytics.database.mssql.Migrations
{
    public partial class First_Analytics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "TS3Clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UniqueIdentifier = table.Column<string>(maxLength: 28, nullable: false),
                    DatabaseId = table.Column<int>(nullable: false),
                    NickName = table.Column<string>(nullable: true),
                    TS3Version = table.Column<string>(nullable: true),
                    TS3Plattform = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    LastConnected = table.Column<DateTime>(nullable: false),
                    TotalConnectionCount = table.Column<int>(nullable: false),
                    ChangeDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TS3Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TS3ClientConnection",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ClientGuid = table.Column<Guid>(nullable: false),
                    TimeStamp = table.Column<DateTime>(nullable: false),
                    ChannelId = table.Column<int>(nullable: false),
                    IncactiveSince = table.Column<TimeSpan>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TS3ClientConnection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TS3ClientConnection_TS3Clients_ClientGuid",
                        column: x => x.ClientGuid,
                        principalTable: "TS3Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TS3ClientConnection_ClientGuid",
                table: "TS3ClientConnection",
                column: "ClientGuid");

            migrationBuilder.CreateIndex(
                name: "IX_TS3Clients_UniqueIdentifier",
                table: "TS3Clients",
                column: "UniqueIdentifier",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TS3ClientConnection");

            migrationBuilder.DropTable(
                name: "TS3Clients");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true,
                filter: "[Username] IS NOT NULL");
        }
    }
}
