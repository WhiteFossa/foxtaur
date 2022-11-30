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
    public bool IsInSegment(float lat, float lon)
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