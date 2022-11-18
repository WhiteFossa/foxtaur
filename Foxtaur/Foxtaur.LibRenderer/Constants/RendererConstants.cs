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
    public const float GeoCoordinatesCheckDelta = 0.01f;

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

    /// <summary>
    /// Camera zoom must be greater than this
    /// </summary>
    public const float CameraMaxZoom = 0.0f;

    /// <summary>
    /// Camera zoom must be lesser than this
    /// </summary>
    //public const float CameraMinZoom = (float)Math.PI / 2.0f;
    public const float CameraMinZoom = (float)Math.PI - 0.1f;

    /// <summary>
    /// Camera zoom in multiplier
    /// </summary>
    public const float CameraZoomInMultiplier = 0.95f;

    /// <summary>
    /// Camera zoom out multiplier
    /// </summary>
    public const float CameraZoomOutMultiplier = 1.05f;

    /// <summary>
    /// Camera orbit height
    /// </summary>
    public const float CameraOrbitHeight = 2.0f * EarthRadius;
}