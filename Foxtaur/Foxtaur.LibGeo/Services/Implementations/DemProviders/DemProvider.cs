using Foxtaur.LibGeo.Constants;
using Foxtaur.LibGeo.Models;
using Foxtaur.LibGeo.Services.Abstractions.DemProviders;
using Foxtaur.LibGeo.Services.Abstractions.Readers;
using Foxtaur.LibGeo.Services.Implementations.Readers;

namespace Foxtaur.LibGeo.Services.Implementations.DemProviders;

public class DemProvider : IDemProvider
{
    private IGeoTiffReader _demReader;

    public DemProvider()
    {
        _demReader = new GeoTiffReader();
        _demReader.Open(@"Resources/DEMs/30n000e_20101117_gmted_mea075_scaled.tif");
    }
    
    public float GetSurfaceAltitude(float lat, float lon)
    {
        var h = _demReader.GetPixel(1, new GeoPoint(lat, lon, 0));
        if (h == null)
        {
            return GeoConstants.EarthRadius;
        }

        return GeoConstants.EarthRadius + 0.2f * (h.Value - 0.5f);
    }
}