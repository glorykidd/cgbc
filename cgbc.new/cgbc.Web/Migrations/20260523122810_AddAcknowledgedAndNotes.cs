using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cgbc.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAcknowledgedAndNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAcknowledged",
                table: "ConnectionCards",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ConnectionCardNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConnectionCardId = table.Column<int>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionCardNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConnectionCardNotes_ConnectionCards_ConnectionCardId",
                        column: x => x.ConnectionCardId,
                        principalTable: "ConnectionCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionCardNotes_ConnectionCardId",
                table: "ConnectionCardNotes",
                column: "ConnectionCardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConnectionCardNotes");

            migrationBuilder.DropColumn(
                name: "IsAcknowledged",
                table: "ConnectionCards");
        }
    }
}
