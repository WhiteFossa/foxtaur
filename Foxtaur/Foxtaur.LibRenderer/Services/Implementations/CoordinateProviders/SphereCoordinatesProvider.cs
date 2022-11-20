using Foxtaur.LibRenderer.Constants;
using Foxtaur.LibRenderer.Helpers;
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
        return new PlanarPoint2D(geo.Lon / (2.0f * (float)Math.PI) + 0.5f, 1 - (geo.Lat / (float)Math.PI + 0.5f));
    }

    public PlanarPoint3D GeoToPlanar3D(GeoPoint geo)
    {
        var tmpLon = GeoPoint.SumLongitudesWithWrap(geo.Lon, (float)Math.PI / 2.0f);

        var x = geo.H * (float)Math.Cos(geo.Lat) * (float)Math.Cos(tmpLon);
        var z = geo.H * (float)Math.Cos(geo.Lat) * (float)Math.Sin(tmpLon);
        var y = geo.H * (float)Math.Sin(geo.Lat);

        return new PlanarPoint3D(x + RendererConstants.EarthCenter.X, y + RendererConstants.EarthCenter.Y,
            z + RendererConstants.EarthCenter.Z);
    }

    public GeoPoint Planar2DToGeo(PlanarPoint2D planar2d)
    {
        throw new NotImplementedException();
    }

    public GeoPoint Planar3DToGeo(PlanarPoint3D planar3d)
    {
        var h = planar3d.DistanceTo(RendererConstants.EarthCenter.AsPlanarPoint3D());

        var lat = (float)Math.Asin((planar3d.Y - RendererConstants.EarthCenter.Y) / h);
        var lon = (float)Math.Atan2(planar3d.Z - RendererConstants.EarthCenter.Z,
            planar3d.X - RendererConstants.EarthCenter.X);

        return new GeoPoint(lat, lon, h);
    }
}