using System;
using System.Collections.Generic;

namespace IeltsTestWeb.Models;

public partial class MatchQuestionList
{
    public int MqlistId { get; set; }

    public int QlistId { get; set; }

    public string ChoiceList { get; set; } = null!;

    public virtual QuestionList Qlist { get; set; } = null!;
}
