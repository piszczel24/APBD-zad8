using APBD_zad8.Context;
using Microsoft.AspNetCore.Mvc;

namespace APBD_zad8.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TripsController : ControllerBase
{
    private readonly MyDbContext _dbContext = new();

    [HttpGet]
    public IActionResult GetTripsSortedByDateDescending(int page = 1, int pageSize = 10)
    {
        var trips = _dbContext.Trips.OrderByDescending(t => t.DateFrom).ToList().Skip((page - 1) * pageSize)
            .Take(pageSize).ToList();

        var allPages = (int)Math.Ceiling(_dbContext.Trips.Count() / (double)pageSize);

        var result = new
        {
            pageNum = page,
            pageSize,
            allPages,
            trips
        };

        return Ok(result);
    }

    [HttpDelete("{idClient:int}")]
    public IActionResult DeleteClient(int idClient)
    {
        var client = _dbContext.Clients.Find(idClient);
        if (client == null) return StatusCode(StatusCodes.Status400BadRequest);
        var tripsCount = _dbContext.ClientTrips.Count(ct => ct.IdClient == idClient);
        if (tripsCount != 0) return BadRequest("Nie można usunąć klienta który ma przypisane wycieczki!!!");

        _dbContext.Clients.Remove(client);
        _dbContext.SaveChanges();

        return StatusCode(StatusCodes.Status204NoContent);
    }
}