using System;
using System.Collections.Generic;

namespace IeltsTestWeb.Models;

public partial class Test
{
    public int TestId { get; set; }

    public string TestType { get; set; } = null!;

    public string TestSkill { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int MonthEdition { get; set; }

    public int YearEdition { get; set; }

    public int? UserCompletedNum { get; set; }

    public virtual ICollection<ReadingSection> ReadingSections { get; set; } = new List<ReadingSection>();

    public virtual ICollection<Sound> Sounds { get; set; } = new List<Sound>();
}
