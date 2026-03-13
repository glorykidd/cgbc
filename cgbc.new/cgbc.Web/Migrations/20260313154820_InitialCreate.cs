using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cgbc.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConnectionCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    VisitStatus = table.Column<string>(type: "TEXT", nullable: false),
                    WantsContact = table.Column<bool>(type: "INTEGER", nullable: false),
                    PreferredCommunication = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: true),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    ContactReason = table.Column<string>(type: "TEXT", nullable: false),
                    ContactReasonOther = table.Column<string>(type: "TEXT", nullable: true),
                    PrayerRequests = table.Column<string>(type: "TEXT", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionCards", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionCards_IsRead",
                table: "ConnectionCards",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionCards_SubmittedAt",
                table: "ConnectionCards",
                column: "SubmittedAt",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConnectionCards");
        }
    }
}
