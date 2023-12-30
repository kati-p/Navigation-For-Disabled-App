using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NavigateForDisabledApp.Models;
using System.Security;

namespace NavigateForDisabledApp.Controllers;

[ApiController]
[Route("[controller]")]
public class StationsController : ControllerBase
{
      private readonly ILogger<StationsController> _logger;

      public StationsController(ILogger<StationsController> logger)
      {
            _logger = logger;
      }

      [HttpGet]
      [Route("")]
      [Authorize(Roles = "user")]
      public IActionResult GetAll()
      {
            var db = new NavigateSoftwareDbContext();
            var userID = User?.Identity?.Name;
            if (userID == null) return Unauthorized();
            var stations = db.Stations
            .Select(s => new {
               station_ID = s.StationId,
               station_name = s.StationName
            })
            .ToList();

            if (!stations.Any()) return NoContent();

            return Ok(stations);
      }

      [HttpGet]
      [Route("{Id}")]
      [Authorize(Roles = "user")]
      public IActionResult Get(uint Id)
      {
            
            var db = new NavigateSoftwareDbContext();
            var userID = User?.Identity?.Name;
            if (userID == null) return Unauthorized();
            var station = db.Stations
            .Where(s => s.StationId == Id)
            .Select(s => new {
                  station_ID = s.StationId,
                  station_Name = s.StationName

            })
            .FirstOrDefault();

            if (station == null) return NoContent();

            return Ok(station);
      }
      
}