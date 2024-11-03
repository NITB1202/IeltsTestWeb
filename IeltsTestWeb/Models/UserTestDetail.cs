using System;
using System.Collections.Generic;

namespace IeltsTestWeb.Models;

public partial class UserTestDetail
{
    public int TdetailId { get; set; }

    public int UtestId { get; set; }

    public int SectionId { get; set; }

    public virtual UserTest Utest { get; set; } = null!;
}
