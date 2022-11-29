namespace Foxtaur.LibGeo.Models;

/// <summary>
/// Segment, limited by coordinates
/// </summary>
public class GeoSegment
{
    /// <summary>
    /// North limit
    /// </summary>
    public float NorthLat { get; private set; }

    /// <summary>
    /// South limit
    /// </summary>
    public float SouthLat { get; private set; }

    /// <summary>
    /// West limit
    /// </summary>
    public float WestLon { get; private set; }

    /// <summary>
    /// East limit
    /// </summary>
    public float EastLon { get; private set; }

    public GeoSegment(float nLat, float wLon, float sLat, float eLon)
    {
        if (nLat <= sLat)
        {
            throw new ArgumentException();
        }

        if (wLon <= eLon)
        {
            throw new ArgumentException();
        }
        
        NorthLat = nLat;
        WestLon = wLon;
        SouthLat = sLat;
        EastLon = eLon;
    }

    /// <summary>
    /// Is point withing segment (H is ignored). Lat / lon wrap is not supported
    /// </summary>
    public bool IsInSegment(GeoPoint point)
    {
        _ = point ?? throw new ArgumentNullException(nameof(point));
        
        if (point.Lat < SouthLat)
        {
            return false;
        }

        if (point.Lat > NorthLat)
        {
            return false;
        }
        
        if (point.Lon > WestLon)
        {
            return false;
        }

        if (point.Lon < EastLon)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Is our segment covered by given segment?
    /// </summary>
    public bool IsCoveredBy(GeoSegment segment)
    {
        _ = segment ?? throw new ArgumentNullException(nameof(segment));
        
        var p1 = new GeoPoint(SouthLat, WestLon, 1.0f);
        var p2 = new GeoPoint(NorthLat, WestLon, 1.0f);
        var p3 = new GeoPoint(NorthLat, EastLon, 1.0f);
        var p4 = new GeoPoint(SouthLat, EastLon, 1.0f);

        return segment.IsInSegment(p1)
               ||
               segment.IsInSegment(p2)
               ||
               segment.IsInSegment(p3)
               ||
               segment.IsInSegment(p4);
    }
}