using System;
using System.Collections.Generic;

namespace IeltsTestWeb.Models;

public partial class UserTest
{
    public int UtestId { get; set; }

    public int AccountId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime? DateCreate { get; set; }

    public string TestType { get; set; } = null!;

    public string TestSkil { get; set; } = null!;

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<UserTestDetail> UserTestDetails { get; set; } = new List<UserTestDetail>();
}
