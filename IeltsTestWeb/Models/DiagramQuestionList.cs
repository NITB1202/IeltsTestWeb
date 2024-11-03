using System;
using System.Collections.Generic;

namespace IeltsTestWeb.Models;

public partial class DiagramQuestionList
{
    public int DqlistId { get; set; }

    public int QlistId { get; set; }

    public string ImageLink { get; set; } = null!;

    public virtual QuestionList Qlist { get; set; } = null!;
}
