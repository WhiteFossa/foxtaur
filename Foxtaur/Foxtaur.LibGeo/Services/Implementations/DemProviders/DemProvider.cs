using Foxtaur.LibGeo.Constants;
using Foxtaur.LibGeo.Services.Abstractions.DemProviders;
using Foxtaur.LibResources.Constants;
using Foxtaur.LibResources.Models;
using Foxtaur.LibResources.Services.Abstractions;
using Foxtaur.LibResources.Services.Implementations;

namespace Foxtaur.LibGeo.Services.Implementations.DemProviders;

public class DemProvider : IDemProvider
{
    private readonly IFragmentedResourcesProvider _demResourcesProvider;

    public event IDemProvider.OnRegenerateDemFragmentHandler? OnRegenerateDemFragment;

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

        //Task.Run(() => fragment.DownloadAsync(OnFragmentLoaded));
        fragment.DownloadAsync(OnFragmentLoaded); // Running in separate task

        var h = fragment.GetHeight(lat, lon);
        if (h == null)
        {
            return GeoConstants.EarthRadius;
        }

        return GeoConstants.EarthRadius + GeoConstants.DemAltitudeMultiplicator * ResourcesConstants.DemScalingFactor * (h.Value - ResourcesConstants.DemSeaLevel);
    }

    public void OnFragmentLoaded(FragmentedResourceBase fragment)
    {
        var demFragment = fragment as DemFragment;
        _ = demFragment ?? throw new InvalidOperationException();

        // Requesting DEM regeneration
        OnRegenerateDemFragment?.Invoke(this, new OnRegenerateDemFragmentArgs(demFragment.NorthLat, demFragment.WestLon, demFragment.SouthLat, demFragment.EastLon));
    }
}