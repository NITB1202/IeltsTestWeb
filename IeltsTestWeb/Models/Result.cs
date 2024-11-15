using System;
using System.Collections.Generic;

namespace IeltsTestWeb.Models;

public partial class Result
{
    public int ResultId { get; set; }

    public int Score { get; set; }

    public int AccountId { get; set; }

    public int TestId { get; set; }

    public string TestAccess { get; set; } = null!;

    public DateTime DateMake { get; set; }

    public TimeOnly CompleteTime { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<ResultDetail> ResultDetails { get; set; } = new List<ResultDetail>();
}
