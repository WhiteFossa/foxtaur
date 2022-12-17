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
        var z = geo.H * Math.Cos(geo.Lat) * Math.Cos(geo.Lon);
        var x = geo.H * Math.Cos(geo.Lat) * Math.Sin(geo.Lon);
        var y = geo.H * Math.Sin(geo.Lat);

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