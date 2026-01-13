using System;
using System.Collections.Generic;

namespace lily.Models;

public class Order
{
    public int OrderID { get; set; }

    public int UserID { get; set; }

    public int ToyID { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime? OrderDate { get; set; }

    public string? Status { get; set; }

    public virtual Toy Toy { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
