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
}