using Foxtaur.LibGeo.Models;

namespace Foxtaur.LibGeo.Services.Abstractions.CoordinateProviders;

/// <summary>
/// Interface to work with geocoordinates
/// </summary>
public interface ICoordinatesProvider
{
    /// <summary>
    /// Geocoordinates to planar point (2D)
    /// </summary>
    PlanarPoint2D GeoToPlanar2D(GeoPoint geo);

    /// <summary>
    /// Geocoordinates to planar point (3D)
    /// </summary>
    PlanarPoint3D GeoToPlanar3D(GeoPoint geo);
    
    /// <summary>
    /// Overload, not requiring to create new GeoPoint
    /// </summary>
    PlanarPoint3D GeoToPlanar3D(double lat, double lon, double h);

    /// <summary>
    /// Planar coordinates (2D) to geocoordinates
    /// </summary>
    GeoPoint Planar2DToGeo(PlanarPoint2D planar2d);

    /// <summary>
    /// Planar coordinates (3D) to geocoordinates
    /// </summary>
    GeoPoint Planar3DToGeo(PlanarPoint3D planar3d);
}