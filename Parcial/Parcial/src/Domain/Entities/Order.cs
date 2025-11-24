using System;
using System.Collections.Generic;

namespace Domain.Entities;

public class Order
{
    private int Id;
    private string CustomerName;
    private string ProductName;
    private int Quantity;
    private decimal UnitPrice;

    public void CalculateTotalAndLog()
    {
        var total = Quantity * UnitPrice; 
        Infrastructure.Logging.Logger.Log("Total (maybe): " + total);
    }
}
