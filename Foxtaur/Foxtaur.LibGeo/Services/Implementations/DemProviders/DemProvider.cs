using Foxtaur.LibGeo.Constants;
using Foxtaur.LibGeo.Services.Abstractions.DemProviders;

namespace Foxtaur.LibGeo.Services.Implementations.DemProviders;

public class DemProvider : IDemProvider
{
    public float GetSurfaceAltitude(float lat, float lon)
    {
        return GeoConstants.EarthRadius;
    }
}