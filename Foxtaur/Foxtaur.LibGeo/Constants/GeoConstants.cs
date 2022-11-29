using System.Numerics;

namespace Foxtaur.LibGeo.Constants;

/// <summary>
/// Geography-related constants
/// </summary>
public static class GeoConstants
{
    /// <summary>
    ///     Earth radius (in 3D coordinates)
    /// </summary>
    public const float EarthRadius = 1.0f;

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
    /// Earth center coordinates
    /// </summary>
    public static readonly Vector3 EarthCenter = new Vector3(0.0f, 0.0f, 0.0f);

    /// <summary>
    /// Multiply visual DEM altitudes by this value
    /// </summary>
    public const float DemAltitudeMultiplicator = 10.0f;
}