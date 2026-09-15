using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExternalIdDemo.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialMfa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserMfas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntraObjectId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TotpSecretEncrypted = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotpEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMfas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserMfas_EntraObjectId",
                table: "UserMfas",
                column: "EntraObjectId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserMfas");
        }
    }
}
