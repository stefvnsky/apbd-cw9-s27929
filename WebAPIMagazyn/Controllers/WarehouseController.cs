using Microsoft.AspNetCore.Mvc;
using WebAPIMagazyn.Models;
using WebAPIMagazyn.Services;

namespace WebAPIMagazyn.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    //koncowka POST
    //odbiera dane z WarehouseProductRequest.cs
    //wywoluje metode AddProductToWarehouseAsync()
    //zwraca 200 OK z IdProductWarehouse albo błąd

    private readonly IDbService _dbService;
    
    //Konstruktor
    public WarehouseController(IDbService dbService)
    {
        _dbService = dbService;
    }
    
    //Endpoint POST: /api/warehouse/manual
    [HttpPost("manual")]
    public async Task<IActionResult> AddProductManual([FromBody] WarehouseProductRequest request)
    {
        try
        {
            int result = await _dbService.AddProductToWarehouseAsync(request);
            return Ok(result); // Zwróć IdProductWarehouse i HTTP 200 OK
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message); // Zwróć HTTP 400 i komunikat
        }
        catch (InvalidOperationException e)
        {
            return Conflict(e.Message); // Zwróć HTTP 409 i komunikat
        }
        catch (Exception e)
        {
            return StatusCode(500, $"Błąd serwera: {e.Message}"); // Zwróć HTTP 500
        }
    }
    
    [HttpPost("procedure")]
    public async Task<IActionResult> AddProductProcedure([FromBody] WarehouseProductRequest request)
    {
        try
        {
            int result = await _dbService.AddProductToWarehouseByProcedureAsync(request);
            return Ok(result); // HTTP 200 + ID zwrócony przez procedurę
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message); // HTTP 400
        }
        catch (InvalidOperationException e)
        {
            return Conflict(e.Message); // HTTP 409
        }
        catch (Exception e)
        {
            return StatusCode(500, $"Server error: {e.Message}"); // HTTP 500
        }
    }
}