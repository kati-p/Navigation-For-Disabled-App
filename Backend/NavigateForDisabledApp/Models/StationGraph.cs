using System;
using System.Collections.Generic;

namespace NavigateForDisabledApp.Models;

public partial class StationGraph
{
    public uint Id { get; set; }

    public uint StationId { get; set; }

    public uint NearbyStationId { get; set; }

    public float Weight { get; set; }
}
