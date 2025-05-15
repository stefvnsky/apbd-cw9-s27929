namespace WebAPIMagazyn.Models;

//klasa modelujaca dane ktore uzytkownik wysle w POST
//zostanie zamieniony na obiekt 
public class WarehouseProductRequest
{
    public int IdProduct {get; set;}
    public int IdWarehouse {get; set;}
    public int Amount { get; set; }
    public DateTime CreatedAt {get; set;}
}