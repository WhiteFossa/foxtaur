using Foxtaur.LibGeo.Models;

namespace Foxtaur.LibGeo.Services.Abstractions.DemProviders;

public class OnRegenerateDemFragmentArgs
{
    /// <summary>
    /// Segment, affected by DEM regeneration
    /// </summary>
    public GeoSegment Segment { get; private set; }

    public OnRegenerateDemFragmentArgs(float nLat, float wLon, float sLat, float eLon)
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
    float GetSurfaceAltitude(float lat, float lon);
    
    /// <summary>
    /// Event for DEM fragment regeneration
    /// </summary>
    event OnRegenerateDemFragmentHandler OnRegenerateDemFragment;
    
    /// <summary>
    /// Called when DEM fragment was regenerated. All DEM users must re-request DEM data for given fragment
    /// </summary>
    delegate void OnRegenerateDemFragmentHandler(object sender, OnRegenerateDemFragmentArgs args);
}