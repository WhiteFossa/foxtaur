namespace Foxtaur.LibResources.Constants;

/// <summary>
/// Resources-related constants
/// </summary>
public class ResourcesConstants
{
    /// <summary>
    /// Put downloaded DEM fragments here (relative to application for now)
    /// </summary>
    public const string LocalDemFragmentsDirectory = "Resources/DEMs/GMTED2010/LowResolution";

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
}