using System;
using System.Collections.Generic;

namespace NavigateForDisabledApp.Models;

public partial class Bus
{
    public uint BusId { get; set; }

    public string BusName { get; set; } = null!;

    public virtual ICollection<StationBu> StationBus { get; set; } = new List<StationBu>();
}
