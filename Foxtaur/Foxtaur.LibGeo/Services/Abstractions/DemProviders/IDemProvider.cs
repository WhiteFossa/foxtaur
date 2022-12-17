using Foxtaur.LibGeo.Models;
using Foxtaur.LibResources.Enums;

namespace Foxtaur.LibGeo.Services.Abstractions.DemProviders;

public class OnRegenerateDemFragmentArgs
{
    /// <summary>
    /// Segment, affected by DEM regeneration
    /// </summary>
    public GeoSegment Segment { get; private set; }

    public OnRegenerateDemFragmentArgs(double nLat, double wLon, double sLat, double eLon)
    {
        Segment = new GeoSegment(nLat, wLon, sLat, eLon);
    }
}

/// <summary>
/// Digital elevation map provider
/// </summary>
public interface IDemProvider
{
    /// <summary>
    /// Get surface altitude (in ideal Earth radiuses)
    /// </summary>
    double GetSurfaceAltitude(double lat, double lon, ZoomLevel desiredZoomLevel);
    
    /// <summary>
    /// Event for DEM fragment regeneration
    /// </summary>
    event OnRegenerateDemFragmentHandler OnRegenerateDemFragment;
    
    /// <summary>
    /// Called when DEM fragment was regenerated. All DEM users must re-request DEM data for given fragment
    /// </summary>
    delegate void OnRegenerateDemFragmentHandler(object sender, OnRegenerateDemFragmentArgs args);
}