using System;
using System.Collections.Generic;

namespace IeltsTestWeb.Models;

public partial class QuestionList
{
    public int QlistId { get; set; }

    public string QlistType { get; set; } = null!;

    public string? Content { get; set; }

    public int Qnum { get; set; }

    public virtual ICollection<DiagramQuestionList> DiagramQuestionLists { get; set; } = new List<DiagramQuestionList>();

    public virtual ICollection<MatchQuestionList> MatchQuestionLists { get; set; } = new List<MatchQuestionList>();

    public virtual ICollection<ListeningSection> Lsections { get; set; } = new List<ListeningSection>();

    public virtual ICollection<ReadingSection> Rsections { get; set; } = new List<ReadingSection>();
}
