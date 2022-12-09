using Foxtaur.LibGeo.Constants;
using Foxtaur.LibGeo.Services.Abstractions.DemProviders;
using Foxtaur.LibResources.Constants;
using Foxtaur.LibResources.Enums;
using Foxtaur.LibResources.Models;
using Foxtaur.LibResources.Services.Abstractions;
using Foxtaur.LibResources.Services.Implementations;
using NLog;

namespace Foxtaur.LibGeo.Services.Implementations.DemProviders;

public class DemProvider : IDemProvider
{
    private readonly IFragmentedResourcesProvider _demResourcesProvider;

    public event IDemProvider.OnRegenerateDemFragmentHandler? OnRegenerateDemFragment;
    
    private object _regenerationLock = new object();

    private object _startDownloadLock = new object();
    
    private Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Zoom levels, ordered from higher to lower Fresolution
    /// </summary>
    private List<ZoomLevel> _orderedZoomLevels = new List<ZoomLevel>()
    {
        ZoomLevel.ZoomLevel2,
        ZoomLevel.ZoomLevel1,
        ZoomLevel.ZoomLevel0
    };

    public DemProvider()
    {
        _demResourcesProvider = new DemResourcesProvider();
    }

    public float GetSurfaceAltitude(float lat, float lon, ZoomLevel desiredZoomLevel)
    {
        lock (this)
        {
            // Returning the best available zoom level
            int desiredZoomLevelIndex = -1;
            for (var zoomLevelIndex = 0; zoomLevelIndex < _orderedZoomLevels.Count; zoomLevelIndex++)
            {
                if (_orderedZoomLevels[zoomLevelIndex] == desiredZoomLevel)
                {
                    desiredZoomLevelIndex = zoomLevelIndex;
                    break;
                }
            }

            DemFragment fragment = null;
            for (var zoomLevelIndex = desiredZoomLevelIndex;
                 zoomLevelIndex < _orderedZoomLevels.Count;
                 zoomLevelIndex++)
            {
                fragment = StartFragmentLoad(lat, lon, _orderedZoomLevels[zoomLevelIndex]);
                if (fragment == null)
                {
                    // We don't have DEM for this coordinates at all
                    return GeoConstants.EarthRadius;
                }

                if (fragment.IsLoaded)
                {
                    // Fragment is ready, go to get coordinates
                    break;
                }

                // Fragment is not ready, maybe lower resolution fragment is ready? We will know it on next iteration
            }

            var h = fragment.GetHeight(lat, lon);
            if (h == null)
            {
                return GeoConstants.EarthRadius;
            }

            return GeoConstants.EarthRadius + GeoConstants.DemAltitudeMultiplicator *
                ResourcesConstants.DemScalingFactor * (h.Value - ResourcesConstants.DemSeaLevel);
        }
    }

    private DemFragment? StartFragmentLoad(float lat, float lon, ZoomLevel zoomLevel)
    {
        // Searching for fragment
        var fragment = _demResourcesProvider.GetResource(lat, lon, zoomLevel) as DemFragment;
        if (fragment == null)
        {
            return null;
        }

        lock (_startDownloadLock)
        {
            //fragment.DownloadAsync(OnFragmentLoaded);
            Task.Run(() => fragment.DownloadAsync(OnFragmentLoaded)); // Running in separate thread
        }

        return fragment;
    }

    public void OnFragmentLoaded(FragmentedResourceBase fragment)
    {
        var demFragment = fragment as DemFragment;
        _ = demFragment ?? throw new InvalidOperationException();

        // Requesting DEM regeneration
        lock (_regenerationLock)
        {
            OnRegenerateDemFragment?.Invoke(this, new OnRegenerateDemFragmentArgs(demFragment.NorthLat, demFragment.WestLon, demFragment.SouthLat, demFragment.EastLon));    
        }
    }
}