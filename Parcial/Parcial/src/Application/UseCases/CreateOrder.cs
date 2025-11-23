using System;
using System.Data;
using Microsoft.Data.SqlClient;

using Domain.Entities;     
using Domain.Services;     
using Infrastructure.Data; 
using Infrastructure.Logging;

namespace Application.UseCases;

public class CreateOrderUseCase
{
    public Order Execute(string customer, string product, int qty, decimal price)
    {
        Logger.Log("CreateOrderUseCase starting");

        var order = OrderService.CreateTerribleOrder(customer, product, qty, price);

        const string sql = @"
            INSERT INTO Orders (Id, Customer, Product, Qty, Price)
            VALUES (@id, @customer, @product, @qty, @price);";

        var parameters = new[]
        {
            new SqlParameter("@id", SqlDbType.Int) { Value = order.Id },
            new SqlParameter("@customer", SqlDbType.NVarChar, 100) { Value = customer },
            new SqlParameter("@product", SqlDbType.NVarChar, 100)  { Value = product },
            new SqlParameter("@qty", SqlDbType.Int) { Value = qty },
            new SqlParameter("@price", SqlDbType.Decimal) { Value = price }
        };

        Logger.Try(() => BadDb.ExecuteNonQuery(sql, parameters));

        return order;
    }
}
