using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranslationManager.API.Migrations
{
    /// <inheritdoc />
    public partial class FixUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_translations_resource_key",
                table: "translations");

            migrationBuilder.CreateIndex(
                name: "IX_translations_resource_key_platform",
                table: "translations",
                columns: new[] { "resource_key", "platform" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_translations_resource_key_platform",
                table: "translations");

            migrationBuilder.CreateIndex(
                name: "IX_translations_resource_key",
                table: "translations",
                column: "resource_key",
                unique: true);
        }
    }
}
