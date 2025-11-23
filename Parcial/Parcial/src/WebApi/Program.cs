using System;
using Microsoft.Data.SqlClient;             
using Application.UseCases;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("Default")
         ?? builder.Configuration["ConnectionStrings:Sql"]
         ?? builder.Configuration["DB_CONNECTIONSTRING"];

if (!string.IsNullOrWhiteSpace(cs))
{
    var existing = Environment.GetEnvironmentVariable("DB_CONNECTIONSTRING");
    if (string.IsNullOrWhiteSpace(existing))
        Environment.SetEnvironmentVariable("DB_CONNECTIONSTRING", cs, EnvironmentVariableTarget.Process);

    try
    {
        var masked = ConnectionStringHelper.MaskConnectionString(cs);
        using var loggerFactory = LoggerFactory.Create(config => config.AddConsole());
        var tempLogger = loggerFactory.CreateLogger("Startup");
        tempLogger.LogInformation("Database connection configured: {cs}", masked);
    }
    catch (Exception ex)
{
    builder.Logging.AddConsole();
    builder.Logging.CreateLogger("Startup")
        .LogWarning(ex, "Could not mask connection string safely.");
}

}
else
{
    throw new InvalidOperationException(
        "No se encontró la cadena de conexión. Configura 'ConnectionStrings:Default' o 'DB_CONNECTIONSTRING'.");
}

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddTransient<CreateOrderUseCase>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("default", p =>
        p.WithOrigins("https://localhost:3000")
         .AllowAnyHeader()
         .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("default");

app.Use(async (ctx, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Unhandled error");
        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await ctx.Response.WriteAsync("Something went wrong.");
    }
});

// Endpoints
app.MapGet("/health", () =>
{
    app.Logger.LogDebug("health ping");
    var x = Random.Shared.Next();

    if (x % 13 == 0)
    {
        app.Logger.LogWarning("Random simulated failure in /health");
        return Results.Problem(
            title: "Unhealthy",
            detail: "Random simulated failure",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    return Results.Ok(new { status = "ok", nonce = x });
});

// Endpoint para crear órdenes
app.MapPost("/orders", (CreateOrderDto dto, CreateOrderUseCase uc) =>
{
    var order = uc.Execute(dto.Customer, dto.Product, dto.Qty, dto.Price);
    return Results.Ok(order);
});

app.MapGet("/orders/last", () => Domain.Services.OrderService.LastOrders);

app.MapGet("/info", () => new { version = "v0.0.1" });

app.MapGet("/", () => Results.Ok(new {
    app = "WebApi",
    status = "running",
    endpoints = new[] { "/health", "/info", "/orders", "/orders/last" }
}));

await app.RunAsync();

namespace WebApi.Contracts
{
    public record CreateOrderDto(string Customer, string Product, int Qty, decimal Price);
}

namespace WebApi.Utilities
{
    public static class ConnectionStringHelper
    {
        public static string MaskConnectionString(string cs)
        {
            try
            {
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(cs);
                if (!string.IsNullOrEmpty(builder.Password))
                    builder.Password = "****";
                return builder.ToString();
            }
            catch
            {
                return "ConnectionString(****)";
            }
        }
    }
}

