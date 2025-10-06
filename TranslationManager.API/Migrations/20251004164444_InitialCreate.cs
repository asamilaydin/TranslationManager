using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TranslationManager.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "translations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    resource_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    en = table.Column<string>(type: "text", nullable: false),
                    tr = table.Column<string>(type: "text", nullable: true),
                    de = table.Column<string>(type: "text", nullable: true),
                    platform = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    mobile_synced = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_translations", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "translations",
                columns: new[] { "id", "created_at", "de", "en", "mobile_synced", "platform", "resource_key", "tr", "updated_at" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Willkommen in unserer Anwendung!", "Welcome to our application!", false, "Backend", "welcome.message", "Uygulamamıza hoş geldiniz!", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Anmelden", "Login", false, "Backend", "login.title", "Giriş", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Dashboard", "Dashboard", false, "Backend", "dashboard.title", "Kontrol Paneli", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Speichern", "Save", true, "Android/iOS", "button.save", "Kaydet", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Abbrechen", "Cancel", true, "Android/iOS", "button.cancel", "İptal", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_translations_platform",
                table: "translations",
                column: "platform");

            migrationBuilder.CreateIndex(
                name: "IX_translations_resource_key",
                table: "translations",
                column: "resource_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_translations_updated_at",
                table: "translations",
                column: "updated_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "translations");
        }
    }
}
