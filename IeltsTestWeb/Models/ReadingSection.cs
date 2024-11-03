using System;
using System.Collections.Generic;

namespace IeltsTestWeb.Models;

public partial class ReadingSection
{
    public int RsectionId { get; set; }

    public string? ImageLink { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public int TestId { get; set; }

    public virtual Test Test { get; set; } = null!;

    public virtual ICollection<QuestionList> Qlists { get; set; } = new List<QuestionList>();
}
