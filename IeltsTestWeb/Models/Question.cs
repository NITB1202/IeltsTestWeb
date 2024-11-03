using System;
using System.Collections.Generic;

namespace IeltsTestWeb.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public int QlistId { get; set; }

    public string? Content { get; set; }

    public string? ChoiceList { get; set; }

    public string Answer { get; set; } = null!;

    public virtual ICollection<Explanation> Explanations { get; set; } = new List<Explanation>();

    public virtual ICollection<ResultDetail> ResultDetails { get; set; } = new List<ResultDetail>();
}
