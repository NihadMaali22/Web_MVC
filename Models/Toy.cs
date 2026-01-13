using System;
using System.Collections.Generic;

namespace lily.Models;

public class Toy
{
    public int ToyID { get; set; }
    public string ToyName { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int? Stock { get; set; }
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
