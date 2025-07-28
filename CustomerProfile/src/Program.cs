using Scalar.AspNetCore;
using src.Features;
using src.Infrastructure.Extensions;
using src.Infrastructure.External.Messaging;
using src.Shared.Global;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add Service Extensions
string connectionString = StartupValidator.ConnectionString();
builder.Services.AddCustomerDatabaseInfra(connectionString);
builder.Services.AddCustomerRepository();

// Add Messaging Service
builder.Services.AddSMSMessageServices();

builder.Services.AddControllers();
builder.Services.AddFeaturesServices();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
