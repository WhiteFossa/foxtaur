using System.Numerics;
using ImageMagick;

namespace Foxtaur.LibRenderer.Constants;

/// <summary>
///     Various renderer constants
/// </summary>
public static class RendererConstants
{
    /// <summary>
    ///     Add this delta to coordinates check to avoid exceptions in case of non-precise calculations (planar coordinates)
    /// </summary>
    public const float PlanarCoordinatesCheckDelta = 0.001f;

    /// <summary>
    ///     Add this delta to coordinates check to avoid exceptions in case of non-precise calculations (geocoordinates)
    /// </summary>
    public const float GeoCoordinatesCheckDelta = 0.01f;

    /// <summary>
    ///     Minimal possible latitude
    /// </summary>
    public const float MinLat = (float)Math.PI / -2.0f - GeoCoordinatesCheckDelta;

    /// <summary>
    ///     Maximal possible latitude
    /// </summary>
    public const float MaxLat = (float)Math.PI / 2.0f + GeoCoordinatesCheckDelta;

    /// <summary>
    ///     Minimal possible longitude
    /// </summary>
    public const float MinLon = -1.0f * (float)Math.PI - GeoCoordinatesCheckDelta;

    /// <summary>
    ///     Maximal possible longitude
    /// </summary>
    public const float MaxLon = (float)Math.PI + GeoCoordinatesCheckDelta;

    /// <summary>
    ///     Earth radius (in 3D coordinates)
    /// </summary>
    public const float EarthRadius = 1.0f;

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
    public const float CameraOrbitHeight = 2.0f * EarthRadius;

    /// <summary>
    /// Camera height for surface walk mode
    /// </summary>
    public const float SurfaceModeCameraOrbitHeight = EarthRadius + 0.0005f * EarthRadius;

    /// <summary>
    /// Earth center coordinates
    /// </summary>
    public static readonly Vector3 EarthCenter = new Vector3(0.0f, 0.0f, 0.0f);

    /// <summary>
    /// Head rotation speed (latitudal) in surface run mode
    /// </summary>
    public const float SurfaceRunHeadRotationSpeedLat = 0.0005f;

    /// <summary>
    /// Head rotation speed (longitudal) in surface run mode
    /// </summary>
    public const float SurfaceRunHeadRotationSpeedLon = 0.0005f;

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
}