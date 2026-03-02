using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManager.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomFieldDescriptionAndShowInTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomBool1Description",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomBool1ShowInTable",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomBool2Description",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomBool2ShowInTable",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomBool3Description",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomBool3ShowInTable",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomInt1Description",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomInt1ShowInTable",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomInt2Description",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomInt2ShowInTable",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomInt3Description",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomInt3ShowInTable",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomLink1Description",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomLink1ShowInTable",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomLink2Description",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomLink2ShowInTable",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomLink3Description",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomLink3ShowInTable",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomString1Description",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomString1ShowInTable",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomString2Description",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomString2ShowInTable",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomString3Description",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomString3ShowInTable",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomText1Description",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomText1ShowInTable",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomText2Description",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomText2ShowInTable",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomText3Description",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomText3ShowInTable",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomBool1Description",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomBool1ShowInTable",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomBool2Description",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomBool2ShowInTable",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomBool3Description",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomBool3ShowInTable",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomInt1Description",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomInt1ShowInTable",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomInt2Description",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomInt2ShowInTable",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomInt3Description",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomInt3ShowInTable",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomLink1Description",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomLink1ShowInTable",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomLink2Description",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomLink2ShowInTable",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomLink3Description",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomLink3ShowInTable",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomString1Description",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomString1ShowInTable",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomString2Description",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomString2ShowInTable",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomString3Description",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomString3ShowInTable",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomText1Description",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomText1ShowInTable",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomText2Description",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomText2ShowInTable",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomText3Description",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomText3ShowInTable",
                table: "Inventories");
        }
    }
}
