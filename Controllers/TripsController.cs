using APBD_zad8.Context;
using APBD_zad8.DTOs;
using APBD_zad8.Migrations;
using Microsoft.AspNetCore.Mvc;

namespace APBD_zad8.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TripsController(MyDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public IActionResult GetTripsSortedByDateDescending(int page = 1, int pageSize = 10)
    {
        var trips = dbContext.Trips.OrderByDescending(t => t.DateFrom).ToList().Skip((page - 1) * pageSize)
            .Take(pageSize).ToList();

        var allPages = (int)Math.Ceiling(dbContext.Trips.Count() / (double)pageSize);

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
        var client = dbContext.Clients.Find(idClient);
        if (client == null) return StatusCode(StatusCodes.Status400BadRequest);
        var tripsCount = dbContext.ClientTrips.Count(ct => ct.IdClient == idClient);
        if (tripsCount != 0) return BadRequest("Nie można usunąć klienta który ma przypisane wycieczki!!!");

        dbContext.Clients.Remove(client);
        dbContext.SaveChanges();

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpPost("/{idTrip}/clients")]
    public IActionResult AddClientToTrip(int idTrip, ClientDto clientDto)
    {
        var clientWithPesel = dbContext.Clients.FirstOrDefault(c => c.Pesel == clientDto.Pesel);
        if (clientWithPesel != null) return BadRequest("Klient z podanym PESELem istnieje!!!");
        var isClientAlreadyInTrip = dbContext.ClientTrips
            .Any(ct => ct.IdClientNavigation.Pesel == clientDto.Pesel && ct.IdTrip == idTrip);
        if (isClientAlreadyInTrip) return BadRequest("Klient jest już zapisany na tę wycieczkę!!!");
        var trip = dbContext.Trips.Find(idTrip);
        if (trip == null) return BadRequest("Wycieczka o podanym id nie istnieje!!!");
        if (trip.DateFrom <= DateTime.Now)
            return BadRequest("Nie można zapisać się  na wycieczkę, która już się odbyła!!!");

        var paymentDate = clientDto.PaymentDate;

        var client = new Client
        {
            FirstName = clientDto.FirstName,
            LastName = clientDto.LastName,
            Email = clientDto.Email,
            Telephone = clientDto.Telephone,
            Pesel = clientDto.Pesel
        };

        var clientTrip = new ClientTrip
        {
            IdClientNavigation = client,
            IdTrip = idTrip,
            PaymentDate = paymentDate,
            RegisteredAt = DateTime.Now
        };

        dbContext.ClientTrips.Add(clientTrip);
        dbContext.SaveChangesAsync();
        return Ok();
    }
}