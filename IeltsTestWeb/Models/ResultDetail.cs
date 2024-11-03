using System;
using System.Collections.Generic;

namespace IeltsTestWeb.Models;

public partial class ResultDetail
{
    public int DetailId { get; set; }

    public int ResultId { get; set; }

    public int QuestionOrder { get; set; }

    public int QuestionId { get; set; }

    public string UserAnswer { get; set; } = null!;

    public string QuestionState { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;

    public virtual Result Result { get; set; } = null!;
}
