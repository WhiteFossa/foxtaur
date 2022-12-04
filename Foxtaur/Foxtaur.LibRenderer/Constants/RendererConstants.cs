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
    public const float CameraMaxZoom = 0.0f;

    /// <summary>
    /// Camera zoom must be lesser than this
    /// </summary>
    public const float CameraMinZoom = (float)Math.PI / 2.0f;

    /// <summary>
    /// Camera zoom in multiplier
    /// </summary>
    public const float CameraZoomInMultiplier = 0.95f;

    /// <summary>
    /// Camera zoom out multiplier
    /// </summary>
    public const float CameraZoomOutMultiplier = 1.05f;

    /// <summary>
    /// Camera orbit height
    /// </summary>
    public const float CameraOrbitHeight = 2.0f * GeoConstants.EarthRadius;
    
    /// <summary>
    /// Camera height for surface walk mode
    /// </summary>
    public const float SurfaceModeCameraOrbitHeight = GeoConstants.EarthRadius + 0.001f * GeoConstants.EarthRadius;

    /// <summary>
    /// Head rotation speed (latitudal) in surface run mode
    /// </summary>
    public const float SurfaceRunHeadRotationSpeedLat = 0.0005f;

    /// <summary>
    /// Head rotation speed (longitudal) in surface run mode
    /// </summary>
    public const float SurfaceRunHeadRotationSpeedLon = 0.00005f;

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
    public const float UiFpsTextXPosition = 10;

    /// <summary>
    /// Mouse coords text X position
    /// </summary>
    public const float UiMouseCoordsTextXPosition = 10;

    #endregion

    /// <summary>
    /// Precision for finding is point on ray
    /// </summary>
    public const float TestIsPointOnRayPrecision = 0.00001f;
    

    /// <summary>
    /// Regenerate no more than this amount of segments per frame
    /// </summary>
    public const int MaxSegmentsPerFrameRegeneration = 2;

    /// <summary>
    /// Virtual viewport size for the purporse of culling (because of the strange bug exact 1.0f is not big enough)
    /// </summary>
    public const float CullingViewportSize = 1.5f;
    
    /// <summary>
    /// Underground plane height for surface walk mode segments culling
    /// </summary>
    public const float SurfaceWalkUndergroundPlaneHeight = GeoConstants.EarthRadius - 0.005f * GeoConstants.EarthRadius;
}