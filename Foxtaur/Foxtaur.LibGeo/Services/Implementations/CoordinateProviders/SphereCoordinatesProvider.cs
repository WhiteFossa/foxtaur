using Foxtaur.LibGeo.Constants;
using Foxtaur.LibGeo.Helpers;
using Foxtaur.LibGeo.Models;
using Foxtaur.LibGeo.Services.Abstractions.CoordinateProviders;

namespace Foxtaur.LibGeo.Services.Implementations.CoordinateProviders;

/// <summary>
///     Sphere (ideal Earth) coordinates provider
/// </summary>
public class SphereCoordinatesProvider : ISphereCoordinatesProvider
{
    public PlanarPoint2D GeoToPlanar2D(GeoPoint geo)
    {
        return new PlanarPoint2D(-0.5 * (geo.Lon / Math.PI - 1.0), 1 - (geo.Lat / Math.PI + 0.5));
    }

    public PlanarPoint3D GeoToPlanar3D(GeoPoint geo)
    {
        return GeoToPlanar3D(geo.Lat, geo.Lon, geo.H);
    }

    public PlanarPoint3D GeoToPlanar3D(double lat, double lon, double h)
    {
        var z = h * Math.Cos(lat) * Math.Cos(lon);
        var x = h * Math.Cos(lat) * Math.Sin(lon);
        var y = h * Math.Sin(lat);

        return new PlanarPoint3D(x + GeoConstants.EarthCenter[0], y + GeoConstants.EarthCenter[1], z + GeoConstants.EarthCenter[2]);
    }

    public GeoPoint Planar2DToGeo(PlanarPoint2D planar2d)
    {
        throw new NotImplementedException();
    }

    public GeoPoint Planar3DToGeo(PlanarPoint3D planar3d)
    {
        var h = planar3d.DistanceTo(GeoConstants.EarthCenter.AsPlanarPoint3D());

        var lat = Math.Asin((planar3d.Y - GeoConstants.EarthCenter[1]) / h);
        var lon = Math.Atan2(planar3d.Z - GeoConstants.EarthCenter[2], planar3d.X - GeoConstants.EarthCenter[0]);
        lon = -1.0 * GeoPoint.SumLongitudesWithWrap(lon, -1.0 * Math.PI / 2.0); // Dirty fix

        return new GeoPoint(lat, lon, h);
    }
}