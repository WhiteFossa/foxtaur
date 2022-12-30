using MathNet.Numerics.LinearAlgebra;

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
    public static readonly Vector<double> EarthCenter = Vector<double>.Build.DenseOfArray(new double[] { 0.0, 0.0, 0.0 });
    
    /// <summary>
    /// Earth radius in meters
    /// </summary>
    public const double MetersPerEarthRadius = 6371000.0;
}