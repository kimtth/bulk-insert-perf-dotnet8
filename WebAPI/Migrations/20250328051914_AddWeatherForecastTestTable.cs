using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportLab.Migrations
{
    /// <inheritdoc />
    public partial class AddWeatherForecastTestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "WeatherForecastTests",
                columns: new[] { "Id", "Date", "Summary", "TemperatureC" },
                values: new object[] { 1, new DateOnly(2025, 3, 28), "Cold", 30 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "WeatherForecastTests",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
