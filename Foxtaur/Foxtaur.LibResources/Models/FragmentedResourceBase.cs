namespace Foxtaur.LibResources.Models;

/// <summary>
/// Fragmented resource base class
/// Fragmented resource is a resource, limited by geocoordinates
/// </summary>
public abstract class FragmentedResourceBase
{
    /// <summary>
    /// North border of a fragment
    /// </summary>
    public float NorthLat { get; private set; }

    /// <summary>
    /// South border of fragment
    /// </summary>
    public float SouthLat { get; private set; }

    /// <summary>
    /// Western border (fragment can't stretch over 180 / -180 line)
    /// </summary>
    public float WestLon { get; private set; }

    /// <summary>
    /// Eastern border
    /// </summary>
    public float EastLon { get; private set; }

    /// <summary>
    /// Unique resource name
    /// </summary>
    public string ResourceName { get; private set; }

    /// <summary>
    /// If true, then RemotePath points to a local file, so no need to download anything
    /// </summary>
    public bool IsLocal { get; private set; }

    public FragmentedResourceBase(float northLat,
        float southLat,
        float westLon,
        float eastLon,
        string resourceName,
        bool isLocal)
    {
        if (string.IsNullOrEmpty(resourceName))
        {
            throw new ArgumentException(nameof(resourceName));
        }

        if (northLat <= southLat)
        {
            throw new ArgumentException("NorthLat must be norther than SouthLat");
        }

        if (westLon <= eastLon)
        {
            throw new ArgumentException("WestLon must be wester than EastLon");
        }

        NorthLat = northLat;
        SouthLat = southLat;
        WestLon = westLon;
        EastLon = eastLon;
        ResourceName = resourceName;
        IsLocal = isLocal;
    }

    /// <summary>
    /// After download resource can be found here
    /// </summary>
    /// <returns></returns>
    public virtual string GetLocalPath()
    {
        if (IsLocal)
        {
            return ResourceName; // For local resources local path == remote path
        }

        throw new NotImplementedException("Please, override me!");
    }

    /// <summary>
    /// Is given coordinates hit the resource
    /// </summary>
    public bool IsHit(float lat, float lon)
    {
        return lat >= SouthLat && lat <= NorthLat && lon >= EastLon && lon <= WestLon;
    }

    /// <summary>
    /// Download resource
    /// </summary>
    public abstract Task DownloadAsync();
}