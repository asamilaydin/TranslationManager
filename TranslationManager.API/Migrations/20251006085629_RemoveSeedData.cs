using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TranslationManager.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "translations",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "translations",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "translations",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "translations",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "translations",
                keyColumn: "id",
                keyValue: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
