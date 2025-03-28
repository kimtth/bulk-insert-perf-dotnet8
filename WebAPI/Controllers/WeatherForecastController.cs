using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportLab.Data;
using ReportLab.Model;

namespace ReportLab.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ApplicationDbContext _context;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<ActionResult<IEnumerable<WeatherForecast>>> GetAsync()
        {
            return await _context.WeatherForecasts.ToListAsync();
        }

        [HttpGet("{id}", Name = "GetWeatherForecastById")]
        public async Task<ActionResult<WeatherForecast>> GetById(int id)
        {
            var forecast = await _context.WeatherForecasts.FindAsync(id);

            if (forecast == null)
            {
                return NotFound();
            }

            return forecast;
        }

        [HttpPost]
        public async Task<ActionResult<WeatherForecast>> Create(WeatherForecast forecast)
        {
            _context.WeatherForecasts.Add(forecast);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = forecast.Id }, forecast);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, WeatherForecast forecast)
        {
            if (id != forecast.Id)
            {
                return BadRequest();
            }

            _context.Entry(forecast).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.WeatherForecasts.AnyAsync(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var forecast = await _context.WeatherForecasts.FindAsync(id);
            if (forecast == null)
            {
                return NotFound();
            }

            _context.WeatherForecasts.Remove(forecast);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
