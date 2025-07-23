using src.Infrastructure.Extensions;
using src.Infrastructure.External.Messaging;
using src.Shared.Global;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add Service Extensions
string connectionString = StartupValidator.ConnectionString();
builder.Services.AddCustomerInfraWithDb(connectionString);

// Add Messaging Service
builder.Services.AddMessageServices();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
