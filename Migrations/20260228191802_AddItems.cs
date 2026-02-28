using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InventoryManager.Migrations
{
    /// <inheritdoc />
    public partial class AddItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryId = table.Column<int>(type: "integer", nullable: false),
                    CustomId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CustomString1 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CustomString2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CustomString3 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CustomText1 = table.Column<string>(type: "text", nullable: true),
                    CustomText2 = table.Column<string>(type: "text", nullable: true),
                    CustomText3 = table.Column<string>(type: "text", nullable: true),
                    CustomInt1 = table.Column<int>(type: "integer", nullable: false),
                    CustomInt2 = table.Column<int>(type: "integer", nullable: false),
                    CustomInt3 = table.Column<int>(type: "integer", nullable: false),
                    CustomBool1 = table.Column<bool>(type: "boolean", nullable: false),
                    CustomBool2 = table.Column<bool>(type: "boolean", nullable: false),
                    CustomBool3 = table.Column<bool>(type: "boolean", nullable: false),
                    CustomLink1 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CustomLink2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CustomLink3 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Items_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_CreatedById",
                table: "Items",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Items_InventoryId",
                table: "Items",
                column: "InventoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Items");
        }
    }
}
