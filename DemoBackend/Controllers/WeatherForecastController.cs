using DemoBackend.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        //roles authorization
        [Authorize(Roles = ApplicationRoles.User)]
        [HttpGet]
        [Route("user-weather")]
        public IActionResult GetUserWeather()
        {
            return Ok(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = $"Only the {ApplicationRoles.User} has access to this"
            }).ToArray());
        }

        [Authorize(Roles = ApplicationRoles.Admin)]
        [HttpGet]
        [Route("admin-weather")]
        public IActionResult GetAdminWeather()
        {
            return Ok(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = $"Only the {ApplicationRoles.Admin} has access to this"
            })
            .ToArray());
        }

        //claims based authorization
        [Authorize(Policy = PoliciesConstants.Premium)]
        [HttpGet]
        [Route("premium-weather")]
        public IActionResult GetPremiumWeather()
        {
            return Ok(Enumerable.Range(1, 7).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = $"This is premium weather information"
            })
            .ToArray());
        }

        //policy based authorization (must be at least 18 to access)
        [Authorize(Policy = PoliciesConstants.AgeRestriction)]
        [HttpGet]
        [Route("age-restricted-weather")]
        public IActionResult GetAgeRestrictedWeather()
        {
            return Ok(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = $"This wheather info is age restricted"
            })
            .ToArray());
        }
    }
}
