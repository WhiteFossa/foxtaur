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
        return new PlanarPoint2D(geo.Lon / (2.0f * (float)Math.PI) + 0.5f, 1 - (geo.Lat / (float)Math.PI + 0.5f));
    }

    public PlanarPoint3D GeoToPlanar3D(GeoPoint geo)
    {
        var tmpLon = GeoPoint.SumLongitudesWithWrap(geo.Lon, (float)Math.PI / 2.0f);

        var x = geo.H * (float)Math.Cos(geo.Lat) * (float)Math.Cos(tmpLon);
        var z = geo.H * (float)Math.Cos(geo.Lat) * (float)Math.Sin(tmpLon);
        var y = geo.H * (float)Math.Sin(geo.Lat);

        return new PlanarPoint3D(x + GeoConstants.EarthCenter.X, y + GeoConstants.EarthCenter.Y,
            z + GeoConstants.EarthCenter.Z);
    }

    public GeoPoint Planar2DToGeo(PlanarPoint2D planar2d)
    {
        throw new NotImplementedException();
    }

    public GeoPoint Planar3DToGeo(PlanarPoint3D planar3d)
    {
        var h = planar3d.DistanceTo(GeoConstants.EarthCenter.AsPlanarPoint3D());

        var lat = (float)Math.Asin((planar3d.Y - GeoConstants.EarthCenter.Y) / h);
        var lon = (float)Math.Atan2(planar3d.Z - GeoConstants.EarthCenter.Z, planar3d.X - GeoConstants.EarthCenter.X);
        lon = -1.0f * GeoPoint.SumLongitudesWithWrap(lon, -1.0f * (float)Math.PI / 2.0f); // Dirty fix

        return new GeoPoint(lat, lon, h);
    }
}