using System;
using System.Collections.Generic;

namespace NavigateForDisabledApp.Models;

public partial class StationBu
{
    public uint Id { get; set; }

    public uint StationId { get; set; }

    public uint BusId { get; set; }

    public virtual Bus Bus { get; set; } = null!;

    public virtual Station Station { get; set; } = null!;
}
