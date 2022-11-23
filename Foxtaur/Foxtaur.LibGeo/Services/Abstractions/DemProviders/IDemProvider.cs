namespace Foxtaur.LibGeo.Services.Abstractions.DemProviders;

/// <summary>
/// Digital elevation map provider
/// </summary>
public interface IDemProvider
{
    /// <summary>
    /// Get surface altitude (in ideal Earth radiuses)
    /// </summary>
    float GetSurfaceAltitude(float lat, float lon);
}