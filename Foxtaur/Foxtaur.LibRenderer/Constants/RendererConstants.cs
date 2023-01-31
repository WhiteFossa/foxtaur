using Foxtaur.LibGeo.Constants;
using ImageMagick;

namespace Foxtaur.LibRenderer.Constants;

/// <summary>
///     Various renderer constants
/// </summary>
public static class RendererConstants
{
    /// <summary>
    /// Camera zoom must be greater than this
    /// </summary>
    public const double CameraMaxZoom = 0.0;

    /// <summary>
    /// Camera zoom must be lesser than this
    /// </summary>
    public const double CameraMinZoom = Math.PI / 2.0 - 0.001;

    /// <summary>
    /// Min zoom for surcace run mode
    /// </summary>
    public const double SurfaceRunMinZoom = 1.0;

    /// <summary>
    /// Camera zoom in multiplier
    /// </summary>
    public const double CameraZoomInMultiplier = 0.95;

    /// <summary>
    /// Camera zoom out multiplier
    /// </summary>
    public const double CameraZoomOutMultiplier = 1.05;

    /// <summary>
    /// Camera orbit height
    /// </summary>
    public const double CameraOrbitHeight = 1.5 * GeoConstants.EarthRadius;
    
    /// <summary>
    /// Camera height for surface walk mode
    /// </summary>
    public const double SurfaceRunModeCameraOrbitHeight = 0.00001 * GeoConstants.EarthRadius;

    /// <summary>
    /// Head rotation speed (latitudal) in surface run mode
    /// </summary>
    public const double SurfaceRunHeadRotationSpeedLat = 0.001;

    /// <summary>
    /// Head rotation speed (longitudal) in surface run mode
    /// </summary>
    public const double SurfaceRunHeadRotationSpeedLon = 0.0001;

    #region UI

    /// <summary>
    /// UI top panel height in pixels
    /// </summary>
    public const int UiTopPanelHeight = 40;

    /// <summary>
    /// UI bottom panel height in pixels
    /// </summary>
    public const int UiBottomPanelHeight = 40;

    /// <summary>
    /// UI text color
    /// </summary>
    public static readonly MagickColor UiTextColor = new MagickColor(255, 255, 255, 255);

    /// <summary>
    /// UI panels background color
    /// </summary>
    public static readonly MagickColor UiPanelsBackgroundColor = new MagickColor(30, 30, 60, 200);

    /// <summary>
    /// UI font path. !! USE RASTER FONTS ONLY !!
    /// </summary>
    public const string UiFontPath = @"Resources/Fonts/helvR-100dpi-34.otb";

    /// <summary>
    /// UI font size
    /// </summary>
    public const int UiFontSize = 34;

    /// <summary>
    /// FPS text X position
    /// </summary>
    public const double UiFpsTextXPosition = 10;

    /// <summary>
    /// Mouse coords text X position
    /// </summary>
    public const double UiBottomTextXPosition = 10;

    #endregion

    /// <summary>
    /// Precision for finding is point on ray
    /// </summary>
    public const double TestIsPointOnRayPrecision = 0.00001;

    /// <summary>
    /// Underground plane height for surface walk mode segments culling
    /// </summary>
    public const double SurfaceRunModeUndergroundPlaneHeight = GeoConstants.EarthRadius - 0.005 * GeoConstants.EarthRadius;

    /// <summary>
    /// Maps are higher than Earth surface by this value (satellite mode)
    /// </summary>
    public const double MapsAltitudeIncrementSatelliteMode = 0.009;
    
    /// <summary>
    /// Maps are higher than Earth surface by this value (surface run mode)
    /// </summary>
    public const double MapsAltitudeIncrementSurfaceRunMode = 0.000001;

    /// <summary>
    /// Do not show segments, further than given distance (in Earth's radiuses) (for surface run mode)
    /// </summary>
    public const double SurfaceRunSegmentsCullingDistance = 0.01;

    /// <summary>
    /// Number of Earth segments regeneration threads
    /// </summary>
    public const int SegmentsRegenerationThreads = 1;

    /// <summary>
    /// If we have some free segment regeneration threads, start no more than this amount of new ones
    /// </summary>
    public const int SegmentsRegenerationLimit = 1;

    /// <summary>
    /// Regenerate no more than this number of buffers per frame
    /// </summary>
    public const int RegenerateBuffersPerFrame = 32;

    /// <summary>
    /// Hunter height in Earth radiuses
    /// </summary>
    public const double HunterHeight = 0.000005;

    /// <summary>
    /// Hunter radius in Earth radiuses
    /// </summary>
    public const double HunterRadius = 0.000001;

    /// <summary>
    /// Hunter half width (in parts of the screen) for flat UI markers
    /// </summary>
    public const double FlatUiHunterHalfWidth = 0.02;
    
    /// <summary>
    /// Hunter height (in parts of the screen) for flat UI markers
    /// </summary>
    public const double FlatUiHunterHeight = 0.04;
}