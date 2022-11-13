namespace Foxtaur.LibRenderer.Constants;

/// <summary>
/// Various renderer constants
/// </summary>
public static class RendererConstants
{
    /// <summary>
    /// Add this delta to coordinates check to avoid exceptions in case of non-precise calculations (planar coordinates)
    /// </summary>
    private const float PlanarCoordinatesCheckDelta = 0.001f;
    
    /// <summary>
    /// Add this delta to coordinates check to avoid exceptions in case of non-precise calculations (geocoordinates)
    /// </summary>
    private const float GeoCoordinatesCheckDelta = 0.001f;
    
    /// <summary>
    /// Minimal possible planar coordinate value
    /// </summary>
    public const float MinPlanarCoord = 0.0f - PlanarCoordinatesCheckDelta;
    
    /// <summary>
    /// Maximal possible planar coordinate value
    /// </summary>
    public const float MaxPlanarCoord = 1.0f + PlanarCoordinatesCheckDelta;

    /// <summary>
    /// Minimal possible latitude
    /// </summary>
    public const float MinLat = (float)Math.PI / -2.0f - GeoCoordinatesCheckDelta;
    
    /// <summary>
    /// Maximal possible latitude
    /// </summary>
    public const float MaxLat = (float)Math.PI / 2.0f + GeoCoordinatesCheckDelta;

    /// <summary>
    /// Minimal possible longitude
    /// </summary>
    public const float MinLon = -1.0f * (float)Math.PI - GeoCoordinatesCheckDelta;
    
    /// <summary>
    /// Maximal possible longitude
    /// </summary>
    public const float MaxLon = (float)Math.PI + GeoCoordinatesCheckDelta;

    /// <summary>
    /// Minimal altitude
    /// </summary>
    public const float MinH = 0.0f - PlanarCoordinatesCheckDelta;
    
    /// <summary>
    /// Maximal altitude
    /// </summary>
    public const float MaxH = 1.0f + PlanarCoordinatesCheckDelta;
}