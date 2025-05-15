using WebAPIMagazyn.Models;

namespace WebAPIMagazyn.Services;

//deklaracja jakie opeacje bedzie mozna wykonac w serwisie
//kazda klasa ktora go implementuje musi miec takie metody

public interface IDbService
{
    //int bo zwraca IdProductWarehouse
    //wymaga danych wejsciowych - obiekt klasy WPR
    Task<int> AddProductToWarehouseAsync(WarehouseProductRequest request);
    
    //ta sama logika za pomoca procedury
    Task<int> AddProductToWarehouseByProcedureAsync(WarehouseProductRequest request);
}