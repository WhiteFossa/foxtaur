namespace Foxtaur.LibRenderer.Constants;

/// <summary>
///     Various renderer constants
/// </summary>
public static class RendererConstants
{
    /// <summary>
    ///     Add this delta to coordinates check to avoid exceptions in case of non-precise calculations (planar coordinates)
    /// </summary>
    public const float PlanarCoordinatesCheckDelta = 0.001f;

    /// <summary>
    ///     Add this delta to coordinates check to avoid exceptions in case of non-precise calculations (geocoordinates)
    /// </summary>
    public const float GeoCoordinatesCheckDelta = 0.001f;

    /// <summary>
    ///     Minimal possible latitude
    /// </summary>
    public const float MinLat = (float)Math.PI / -2.0f - GeoCoordinatesCheckDelta;

    /// <summary>
    ///     Maximal possible latitude
    /// </summary>
    public const float MaxLat = (float)Math.PI / 2.0f + GeoCoordinatesCheckDelta;

    /// <summary>
    ///     Minimal possible longitude
    /// </summary>
    public const float MinLon = -1.0f * (float)Math.PI - GeoCoordinatesCheckDelta;

    /// <summary>
    ///     Maximal possible longitude
    /// </summary>
    public const float MaxLon = (float)Math.PI + GeoCoordinatesCheckDelta;

    /// <summary>
    ///     Earth radius (in 3D coordinates)
    /// </summary>
    public const float EarthRadius = 1.0f;
}