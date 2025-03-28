using ReportLab.Model;
using Microsoft.EntityFrameworkCore;

namespace ReportLab.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<WeatherForecast> WeatherForecasts { get; set; }
        public DbSet<WeatherForecastTest> WeatherForecastTests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the WeatherForecast entity
            modelBuilder.Entity<WeatherForecast>()
                .Property(w => w.Summary)
                .HasMaxLength(100);

            // Configure the WeatherForecastTest entity
            modelBuilder.Entity<WeatherForecastTest>()
                .Property(w => w.Summary)
                .HasMaxLength(100);

            // Seed data if needed
            modelBuilder.Entity<WeatherForecast>().HasData(
                new WeatherForecast
                {
                    Id = 1,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    TemperatureC = 25,
                    Summary = "Warm"
                }
            );
            modelBuilder.Entity<WeatherForecastTest>().HasData(
                new WeatherForecastTest
                {
                    Id = 1,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    TemperatureC = 30,
                    Summary = "Cold"
                }
            );
        }
    }
}
