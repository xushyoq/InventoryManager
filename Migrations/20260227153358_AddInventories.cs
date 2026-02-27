using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InventoryManager.Migrations
{
    /// <inheritdoc />
    public partial class AddInventories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CustomString1State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomString1Name = table.Column<string>(type: "text", nullable: true),
                    CustomString2State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomString2Name = table.Column<string>(type: "text", nullable: true),
                    CustomString3State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomString3Name = table.Column<string>(type: "text", nullable: true),
                    CustomText1State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomText1Name = table.Column<string>(type: "text", nullable: true),
                    CustomText2State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomText2Name = table.Column<string>(type: "text", nullable: true),
                    CustomText3State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomText3Name = table.Column<string>(type: "text", nullable: true),
                    CustomInt1State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomInt1Name = table.Column<string>(type: "text", nullable: true),
                    CustomInt2State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomInt2Name = table.Column<string>(type: "text", nullable: true),
                    CustomInt3State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomInt3Name = table.Column<string>(type: "text", nullable: true),
                    CustomBool1State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomBool1Name = table.Column<string>(type: "text", nullable: true),
                    CustomBool2State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomBool2Name = table.Column<string>(type: "text", nullable: true),
                    CustomBool3State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomBool3Name = table.Column<string>(type: "text", nullable: true),
                    CustomLink1State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomLink1Name = table.Column<string>(type: "text", nullable: true),
                    CustomLink2State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomLink2Name = table.Column<string>(type: "text", nullable: true),
                    CustomLink3State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomLink3Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventories_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_CreatedById",
                table: "Inventories",
                column: "CreatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inventories");
        }
    }
}
