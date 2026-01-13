using System;
using System.Collections.Generic;

namespace lily.Models;

public class User
{
    public int UserID { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? Role { get; set; }
    public DateTime? CreatedDate { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
