using WebAPIMagazyn.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the Container

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<IDbService, DbService>(); //"gdy ktoś potrzebuje IDbService uzyj DbService"

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers(); //mapowanie kontrolerów
app.Run();