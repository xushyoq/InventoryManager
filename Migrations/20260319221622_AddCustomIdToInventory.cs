using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManager.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomIdToInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomIdCounter",
                table: "Inventories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CustomIdTemplate",
                table: "Inventories",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomIdCounter",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomIdTemplate",
                table: "Inventories");
        }
    }
}
