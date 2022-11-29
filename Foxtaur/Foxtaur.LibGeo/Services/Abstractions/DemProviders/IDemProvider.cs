namespace Foxtaur.LibGeo.Services.Abstractions.DemProviders;

public class OnRegenerateDemFragmentArgs
{
    public float NLat { get; private set; }
    
    public float SLat { get; private set; }

    public float WLon { get; private set; }
    
    public float ELon { get; private set; }

    public OnRegenerateDemFragmentArgs(float nLat, float wLon, float sLat, float eLon)
    {
        NLat = nLat;
        SLat = sLat;
        WLon = wLon;
        ELon = eLon;
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