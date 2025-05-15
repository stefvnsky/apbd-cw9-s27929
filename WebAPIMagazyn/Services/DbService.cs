using WebAPIMagazyn.Models;

namespace WebAPIMagazyn.Services;

public class DbService : IDbService
{
    private readonly IConfiguration _configuration;
    public DbService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<int> AddProductToWarehouseAsync(WarehouseProductRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<int> AddProductToWarehouseByProcedureAsync(WarehouseProductRequest request)
    {
        throw new NotImplementedException();
    }
}