using System.Numerics;
using Foxtaur.LibRenderer.Models;
using Foxtaur.LibRenderer.Services.Abstractions.CoordinateProviders;

namespace Foxtaur.LibRenderer.Services.Implementations.CoordinateProviders;

/// <summary>
/// Sphere (ideal Earth) coordinates provider
/// </summary>
public class SphereCoordinatesProvider : ICoordinatesProvider
{
    /// <summary>
    /// Earth radius
    /// </summary>
    private const float Radius = 1.0f;
    
    public PlanarPoint2D GeoToPlanar2D(GeoPoint geo)
    {
        return new PlanarPoint2D()
        {
            X = (geo.Lon - (float)Math.PI) / (-2.0f * (float)Math.PI),
            Y = geo.Lat / (float)Math.PI + 0.5f
        };
    }

    public PlanarPoint3D GeoToPlanar3D(GeoPoint geo)
    {
        var radiusVector = new Vector3(0, 0, geo.H);

        // Longitude
        radiusVector = Vector3.Transform(radiusVector, Quaternion.CreateFromAxisAngle(Vector3.UnitY, geo.Lon));
        
        // Latitude
        radiusVector = Vector3.Transform(radiusVector, Quaternion.CreateFromAxisAngle(Vector3.UnitX, -1 * geo.Lat));

        return new PlanarPoint3D() { X = radiusVector.X, Y = radiusVector.Y, Z = radiusVector.Z};
    }

    public GeoPoint Planar2DToGeo(PlanarPoint2D planar2d)
    {
        throw new NotImplementedException();
    }

    public GeoPoint Planar3DToGeo(PlanarPoint3D planar3d)
    {
        throw new NotImplementedException();
    }
}