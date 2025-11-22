using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MyCRM.Client.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult PublicWeather()
        => Ok("This is public weather data");

    [Authorize]
    [HttpGet("secure")]
    public IActionResult SecureWeather()
        => Ok($"Secure data for {User.Identity!.Name}");
}