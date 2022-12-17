namespace Foxtaur.LibGeo.Models;

/// <summary>
/// Segment, limited by coordinates
/// </summary>
public class GeoSegment
{
    /// <summary>
    /// North limit
    /// </summary>
    public double NorthLat { get; private set; }

    /// <summary>
    /// South limit
    /// </summary>
    public double SouthLat { get; private set; }

    /// <summary>
    /// West limit
    /// </summary>
    public double WestLon { get; private set; }

    /// <summary>
    /// East limit
    /// </summary>
    public double EastLon { get; private set; }

    public GeoSegment(double nLat, double wLon, double sLat, double eLon)
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
    public bool IsInSegment(double lat, double lon)
    {
        if (lat < SouthLat)
        {
            return false;
        }

        if (lat > NorthLat)
        {
            return false;
        }
        
        if (lon > WestLon)
        {
            return false;
        }

        if (lon < EastLon)
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

        return segment.IsInSegment(SouthLat, WestLon)
               ||
               segment.IsInSegment(NorthLat, WestLon)
               ||
               segment.IsInSegment(NorthLat, EastLon)
               ||
               segment.IsInSegment(SouthLat, EastLon);
    }
}