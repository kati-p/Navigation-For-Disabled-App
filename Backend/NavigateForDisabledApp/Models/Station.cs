using System;
using System.Collections.Generic;

namespace NavigateForDisabledApp.Models;

public partial class Station
{
    public uint StationId { get; set; }

    public string StationName { get; set; } = null!;

    public virtual ICollection<StationBu> StationBus { get; set; } = new List<StationBu>();
}
