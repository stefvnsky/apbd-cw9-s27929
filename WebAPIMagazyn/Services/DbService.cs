using System.Data;
using Microsoft.Data.SqlClient;
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
        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await connection.OpenAsync();

        await using var command = new SqlCommand();
        command.Connection = connection;
        
        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = (SqlTransaction)transaction;

        try
        {
            //1. walidacja danych wejsciowych
            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero");
            
            //2. Sprawdzenie czy produkt istnieje
            command.CommandText = "SELECT 1 FROM Product WHERE IdProduct = @IdProduct";
            command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
            
            //zwraca wartosc pierwszej kolumny z pirwszego wiersza - czy istnieje?
            var productExists = await command.ExecuteScalarAsync();
            if (productExists == null)
                throw new ArgumentException("Product not found");
            
            command.Parameters.Clear(); //czyszczenie parametrow
            
            //3. Sprawdzenie czy magazyn istnieje
            command.CommandText = "SELECT 1 FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
            command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
            
            var warehouseExists = await command.ExecuteScalarAsync();
            if (warehouseExists == null)
                throw new ArgumentException("Warehouse not found");
            
            command.Parameters.Clear();
            
            //4. Sprawdzenie czy istnieje zamowienie ktore spelnia warunki
            command.CommandText = @"
                SELECT IdOrder 
                FROM [Order] 
                WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @CreatedAt";
            
            command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
            command.Parameters.AddWithValue("@Amount", request.Amount);
            command.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);
            
            var orderIdObject = await command.ExecuteScalarAsync();
            if (orderIdObject == null)
                    throw new ArgumentException("Order not found");
            
            int idOrder = (int)orderIdObject;
            command.Parameters.Clear();
            
            //5. Sprawdzenie czy zamowienie juz zrealizowane
            command.CommandText = "SELECT 1 FROM Product_Warehouse WHERE IdOrder = @IdOrder";
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            
            var alreadyDone = await command.ExecuteScalarAsync();
            if (alreadyDone != null)
                throw new InvalidOperationException("Order already Done");
            
            command.Parameters.Clear();
            
            //6. Aktualizacja zamowienia na aktualna date
            //GETDATE() -> aktualna data i godzina 
            command.CommandText = @"
                UPDATE [Order] SET FullfilledAt = GETDATE() 
                Where IdOrder = @IdOrder";

            command.Parameters.AddWithValue("@IdOrder", idOrder);
            
            //7. Pobierz cene produktu i dodaj rekord do Product_Warehouse
            command.CommandText = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
            command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
            
            var priceObject = await command.ExecuteScalarAsync();
            var price = (decimal)priceObject;
            
            command.Parameters.Clear();
            
            command.CommandText = @"
                INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                OUTPUT INSERTED.IdProductWarehouse
                VALUES
                    (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, GETDATE())";
            
            command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
            command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@Amount", request.Amount);
            command.Parameters.AddWithValue("@Price", price * request.Amount);

            var insertedIdObject = await command.ExecuteScalarAsync(); 
            int insertedId = (int)insertedIdObject;
            
            //9. zatwierdz transakcje
            await transaction.CommitAsync(); ;
            return insertedId;
        }
        catch (Exception)
        {
            //cofnij wszystkie zmiany, jesli wystapil blad
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<int> AddProductToWarehouseByProcedureAsync(WarehouseProductRequest request)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await connection.OpenAsync();

        await using var command = new SqlCommand("AddProductToWarehouse", connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
        command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
        command.Parameters.AddWithValue("@Amount", request.Amount);
        command.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

        try
        {
            var result = await command.ExecuteScalarAsync();
            if (result == null)
                throw new Exception("Procedure returned no ID");

            return Convert.ToInt32(result);
        }
        catch (SqlException e)
        {
            throw new Exception($"SQL error: {e.Message}");
        }
    }

}