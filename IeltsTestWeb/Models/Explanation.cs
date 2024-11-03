using System;
using System.Collections.Generic;

namespace IeltsTestWeb.Models;

public partial class Explanation
{
    public int ExId { get; set; }

    public string Content { get; set; } = null!;

    public int QuestionId { get; set; }

    public virtual Question Question { get; set; } = null!;
}
