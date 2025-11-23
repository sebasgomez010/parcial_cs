using System;
using System.Collections.Generic;

namespace Domain.Services;

using Domain.Entities;

public static class OrderService
{
    // Campo privado inmutable
    private static readonly List<Order> _lastOrders = new();

    // Propiedad pública solo lectura 
    public static IReadOnlyList<Order> LastOrders => _lastOrders;

    public static Order CreateTerribleOrder(string customer, string product, int qty, decimal price)
    {
        var o = new Order
        {
            Id = new Random().Next(1, 999999),
            CustomerName = customer,
            ProductName = product,
            Quantity = qty,
            UnitPrice = price
        };

        _lastOrders.Add(o);
        Infrastructure.Logging.Logger.Log($"Created order {o.Id} for {customer}");
        return o;
    }
}
