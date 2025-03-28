using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ReportLab.Data;
using ReportLab.Model;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ReportLab.PerfTest
{
    public class BulkInsertTests
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public BulkInsertTests()
        {
            // Load configuration from appsettings.json
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory) // Set base directory
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Load JSON file
                .Build();

            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        // Method to run all tests - this replaces the xUnit [Fact] method
        public async Task RunAllTests(int recordCount = 10000)
        {
            Console.WriteLine($"Running bulk insert tests with {recordCount} records...\n");

            // Run tests on WeatherForecast table
            await RunWeatherForecastTests(recordCount);

            // Run tests on WeatherForecastTest table
            await RunWeatherForecastTestTableTests(recordCount);
        }

        public async Task RunWeatherForecastTests(int recordCount = 10000)
        {
            Console.WriteLine("=== Running tests on WeatherForecast table ===");

            // Create test data
            var weatherForecasts = GenerateTestData(recordCount);

            // Run each test and output results
            await TestEntityFrameworkCoreAddRange(weatherForecasts);
            await TestEntityFrameworkCoreExecuteSqlRaw(weatherForecasts);
            await TestSqlBulkCopy(weatherForecasts);

            Console.WriteLine();
        }

        public async Task RunWeatherForecastTestTableTests(int recordCount = 10000)
        {
            Console.WriteLine("=== Running tests on WeatherForecastTest table ===");

            // Create test data
            var weatherForecasts = GenerateTestDataForTestTable(recordCount);

            // Run each test and output results
            await TestEntityFrameworkCoreAddRangeTestTable(weatherForecasts);
            await TestEntityFrameworkCoreExecuteSqlRawTestTable(weatherForecasts);
            await TestSqlBulkCopyTestTable(weatherForecasts);

            Console.WriteLine();
        }

        public static List<WeatherForecast> GenerateTestData(int count)
        {
            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild",
                "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

            var random = new Random();
            var forecasts = new List<WeatherForecast>();

            for (int i = 0; i < count; i++)
            {
                forecasts.Add(new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(i % 365)),
                    TemperatureC = random.Next(-20, 55),
                    Summary = summaries[random.Next(summaries.Length)]
                });
            }

            return forecasts;
        }

        private List<WeatherForecastTest> GenerateTestDataForTestTable(int count)
        {
            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild",
                "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

            var random = new Random();
            var forecasts = new List<WeatherForecastTest>();

            for (int i = 0; i < count; i++)
            {
                forecasts.Add(new WeatherForecastTest
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(i % 365)),
                    TemperatureC = random.Next(-20, 55),
                    Summary = summaries[random.Next(summaries.Length)]
                });
            }

            return forecasts;
        }

        private async Task TestEntityFrameworkCoreAddRange(List<WeatherForecast> forecasts)
        {
            // Create a new context
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_connectionString)
                .Options;

            using var context = new ApplicationDbContext(options);

            // Clean up any existing data
            await context.Database.ExecuteSqlRawAsync("DELETE FROM WeatherForecasts");

            var stopwatch = Stopwatch.StartNew();

            // Add the forecasts using EF Core's AddRange
            context.WeatherForecasts.AddRange(forecasts);
            await context.SaveChangesAsync();

            stopwatch.Stop();

            Console.WriteLine($"EF Core AddRange: {stopwatch.ElapsedMilliseconds}ms for {forecasts.Count} records");
        }

        private async Task TestEntityFrameworkCoreAddRangeTestTable(List<WeatherForecastTest> forecasts)
        {
            // Create a new context
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_connectionString)
                .Options;

            using var context = new ApplicationDbContext(options);

            // Clean up any existing data
            await context.Database.ExecuteSqlRawAsync("DELETE FROM WeatherForecastTests");

            var stopwatch = Stopwatch.StartNew();

            // Add the forecasts using EF Core's AddRange
            context.WeatherForecastTests.AddRange(forecasts);
            await context.SaveChangesAsync();

            stopwatch.Stop();

            Console.WriteLine($"EF Core AddRange (Test Table): {stopwatch.ElapsedMilliseconds}ms for {forecasts.Count} records");
        }

        private async Task TestEntityFrameworkCoreExecuteSqlRaw(List<WeatherForecast> forecasts)
        {
            // Create a new context
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_connectionString)
                .Options;

            using var context = new ApplicationDbContext(options);

            // Clean up any existing data
            await context.Database.ExecuteSqlRawAsync("DELETE FROM WeatherForecasts");

            var stopwatch = Stopwatch.StartNew();

            // Using string interpolation and parameter creation
            for (int i = 0; i < forecasts.Count; i += 1000) // Process in batches of 1000
            {
                var batch = forecasts.Skip(i).Take(1000).ToList();
                var sql = "INSERT INTO WeatherForecasts (Date, TemperatureC, Summary) VALUES ";
                var values = new List<string>();
                var parameters = new List<object>();

                for (int j = 0; j < batch.Count; j++)
                {
                    values.Add($"(@Date{j}, @TemperatureC{j}, @Summary{j})");
                    parameters.Add(new SqlParameter($"@Date{j}", batch[j].Date.ToDateTime(TimeOnly.MinValue)));
                    parameters.Add(new SqlParameter($"@TemperatureC{j}", batch[j].TemperatureC));
                    parameters.Add(new SqlParameter($"@Summary{j}", (object)batch[j].Summary ?? DBNull.Value));
                }

                sql += string.Join(", ", values);
                await context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());
            }

            stopwatch.Stop();

            Console.WriteLine($"EF Core ExecuteSqlRaw: {stopwatch.ElapsedMilliseconds}ms for {forecasts.Count} records");
        }

        private async Task TestEntityFrameworkCoreExecuteSqlRawTestTable(List<WeatherForecastTest> forecasts)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_connectionString)
                .Options;

            using var context = new ApplicationDbContext(options);

            await context.Database.ExecuteSqlRawAsync("DELETE FROM WeatherForecastTests");

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < forecasts.Count; i += 100) // Process in batches of 700
            {
                var batch = forecasts.Skip(i).Take(100).ToList();
                var sql = "INSERT INTO WeatherForecastTests (Date, TemperatureC, Summary) VALUES ";
                var values = new List<string>();
                var parameters = new List<object>();

                for (int j = 0; j < batch.Count; j++)
                {
                    values.Add($"(@Date{j}, @TemperatureC{j}, @Summary{j})");
                    parameters.Add(new SqlParameter($"@Date{j}", batch[j].Date.ToDateTime(TimeOnly.MinValue)));
                    parameters.Add(new SqlParameter($"@TemperatureC{j}", batch[j].TemperatureC));
                    parameters.Add(new SqlParameter($"@Summary{j}", (object)batch[j].Summary ?? DBNull.Value));
                }

                sql += string.Join(", ", values);
                await context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());
            }

            stopwatch.Stop();

            Console.WriteLine($"EF Core ExecuteSqlRaw (Test Table): {stopwatch.ElapsedMilliseconds}ms for {forecasts.Count} records");
        }

        private async Task TestSqlBulkCopy(List<WeatherForecast> forecasts)
        {
            // Create a DataTable with the same structure as WeatherForecast
            var dataTable = new DataTable();
            dataTable.Columns.Add("Date", typeof(DateTime));
            dataTable.Columns.Add("TemperatureC", typeof(int));
            dataTable.Columns.Add("Summary", typeof(string));

            // Add rows to the DataTable
            foreach (var forecast in forecasts)
            {
                dataTable.Rows.Add(
                    forecast.Date.ToDateTime(TimeOnly.MinValue),
                    forecast.TemperatureC,
                    forecast.Summary
                );
            }

            // Clean up any existing data
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new SqlCommand("DELETE FROM WeatherForecasts", connection);
                await command.ExecuteNonQueryAsync();
            }

            var stopwatch = Stopwatch.StartNew();

            // Use SqlBulkCopy to insert the data
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var bulkCopy = new SqlBulkCopy(connection)
                {
                    DestinationTableName = "WeatherForecasts",
                    BatchSize = 5000,
                    BulkCopyTimeout = 60
                };

                bulkCopy.ColumnMappings.Add("Date", "Date");
                bulkCopy.ColumnMappings.Add("TemperatureC", "TemperatureC");
                bulkCopy.ColumnMappings.Add("Summary", "Summary");

                await bulkCopy.WriteToServerAsync(dataTable);
            }

            stopwatch.Stop();

            Console.WriteLine($"SqlBulkCopy: {stopwatch.ElapsedMilliseconds}ms for {forecasts.Count} records");
        }

        private async Task TestSqlBulkCopyTestTable(List<WeatherForecastTest> forecasts)
        {
            // Create a DataTable with the same structure as WeatherForecastTest
            var dataTable = new DataTable();
            dataTable.Columns.Add("Date", typeof(DateTime));
            dataTable.Columns.Add("TemperatureC", typeof(int));
            dataTable.Columns.Add("Summary", typeof(string));

            // Add rows to the DataTable
            foreach (var forecast in forecasts)
            {
                dataTable.Rows.Add(
                    forecast.Date.ToDateTime(TimeOnly.MinValue),
                    forecast.TemperatureC,
                    forecast.Summary
                );
            }

            // Clean up any existing data
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new SqlCommand("DELETE FROM WeatherForecastTests", connection);
                await command.ExecuteNonQueryAsync();
            }

            var stopwatch = Stopwatch.StartNew();

            // Use SqlBulkCopy to insert the data
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var bulkCopy = new SqlBulkCopy(connection)
                {
                    DestinationTableName = "WeatherForecastTests",
                    BatchSize = 5000,
                    BulkCopyTimeout = 60
                };

                bulkCopy.ColumnMappings.Add("Date", "Date");
                bulkCopy.ColumnMappings.Add("TemperatureC", "TemperatureC");
                bulkCopy.ColumnMappings.Add("Summary", "Summary");

                await bulkCopy.WriteToServerAsync(dataTable);
            }

            stopwatch.Stop();

            Console.WriteLine($"SqlBulkCopy (Test Table): {stopwatch.ElapsedMilliseconds}ms for {forecasts.Count} records");
        }
    }
}
