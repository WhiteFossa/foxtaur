using Foxtaur.LibRenderer.Models;
using Foxtaur.LibRenderer.Services.Abstractions.CoordinateProviders;

namespace Foxtaur.LibRenderer.Services.Implementations.CoordinateProviders;

/// <summary>
///     Sphere (ideal Earth) coordinates provider
/// </summary>
public class SphereCoordinatesProvider : ISphereCoordinatesProvider
{
    public PlanarPoint2D GeoToPlanar2D(GeoPoint geo)
    {
        return new PlanarPoint2D(1 - (geo.Lon / (2.0f * (float)Math.PI) + 0.5f), 1 - (geo.Lat / (float)Math.PI + 0.5f));
    }

    public PlanarPoint3D GeoToPlanar3D(GeoPoint geo)
    {
        var x = geo.H * (float)Math.Cos(geo.Lat) * (float)Math.Cos(geo.Lon);
        var z = -1.0f * geo.H * (float)Math.Cos(geo.Lat) * (float)Math.Sin(geo.Lon);
        var y = geo.H * (float)Math.Sin(geo.Lat);

        return new PlanarPoint3D(x, y, z);
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