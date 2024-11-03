using System;
using System.Collections.Generic;

namespace IeltsTestWeb.Models;

public partial class Account
{
    public int AccountId { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int? RoleId { get; set; }

    public string? AvatarLink { get; set; }

    public decimal? Goal { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<UserTest> UserTests { get; set; } = new List<UserTest>();
}
