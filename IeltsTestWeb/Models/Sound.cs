using System;
using System.Collections.Generic;

namespace IeltsTestWeb.Models;

public partial class Sound
{
    public int SoundId { get; set; }

    public string SoundLink { get; set; } = null!;

    public int TestId { get; set; }

    public virtual ICollection<ListeningSection> ListeningSections { get; set; } = new List<ListeningSection>();

    public virtual Test Test { get; set; } = null!;
}
