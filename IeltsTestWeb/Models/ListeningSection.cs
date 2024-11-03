using System;
using System.Collections.Generic;

namespace IeltsTestWeb.Models;

public partial class ListeningSection
{
    public int LsectionId { get; set; }

    public int SectionOrder { get; set; }

    public TimeOnly TimeStamp { get; set; }

    public string? Transcript { get; set; }

    public int SoundId { get; set; }

    public virtual Sound Sound { get; set; } = null!;

    public virtual ICollection<QuestionList> Qlists { get; set; } = new List<QuestionList>();
}
