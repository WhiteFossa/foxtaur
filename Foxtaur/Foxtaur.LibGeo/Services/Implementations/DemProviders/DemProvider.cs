using Foxtaur.LibGeo.Constants;
using Foxtaur.LibGeo.Services.Abstractions.DemProviders;
using Foxtaur.LibResources.Constants;
using Foxtaur.LibResources.Models;
using Foxtaur.LibResources.Services.Abstractions;
using Foxtaur.LibResources.Services.Implementations;

namespace Foxtaur.LibGeo.Services.Implementations.DemProviders;

public class DemProvider : IDemProvider
{
    private IFragmentedResourcesProvider _demResourcesProvider;

    public DemProvider()
    {
        _demResourcesProvider = new DemResourcesProvider();
    }

    public float GetSurfaceAltitude(float lat, float lon)
    {
        // Searching for fragment
        var fragment = _demResourcesProvider.GetResource(lat, lon) as DemFragment;
        if (fragment == null)
        {
            // We don't have DEM for this coordinates at all
            return GeoConstants.EarthRadius;
        }

        // TODO: Move this into threads pool
        Task.WaitAll(fragment.DownloadAsync());

        var h = fragment.GetHeight(lat, lon);
        if (h == null)
        {
            return GeoConstants.EarthRadius;
        }

        return GeoConstants.EarthRadius + 0.2f * (h.Value - ResourcesConstants.DemSeaLevel);
    }
}