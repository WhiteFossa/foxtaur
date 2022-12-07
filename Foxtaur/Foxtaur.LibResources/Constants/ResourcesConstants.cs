namespace Foxtaur.LibResources.Constants;

/// <summary>
/// Resources-related constants
/// </summary>
public class ResourcesConstants
{
    /// <summary>
    /// Read DEM data from this band
    /// </summary>
    public const int DemBand = 1;

    /// <summary>
    /// This brightness in DEM is equal to the sea level
    /// </summary>
    public const float DemSeaLevel = 0.5f;

    /// <summary>
    /// If DEM brightness is lower than this value, then there is no data
    /// </summary>
    public const float DemNoData = 0.1f;

    /// <summary>
    /// DEM scaling factor (full swipe / Earth radius)
    /// </summary>
    public const float DemScalingFactor = 65.535f / 6371.0f;

    /// <summary>
    /// Base URL for resources
    /// </summary>
    public const string ResourcesBaseUrl = "https://static.foxtaur.me/";

    /// <summary>
    /// Put downloaded resources here (relative to executable)
    /// </summary>
    public const string DownloadedDirectory = "Downloaded/";

    /// <summary>
    /// DEM is here on server
    /// </summary>
    public const string DemRemotePath = "DEMs/GMTED2010/";

    /// <summary>
    /// How many active downloading threads can be
    /// </summary>
    public const int MaxActiveDownloadingThreads = 5;
}