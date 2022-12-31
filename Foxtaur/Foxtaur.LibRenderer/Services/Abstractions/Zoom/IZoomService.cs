using Foxtaur.LibResources.Enums;

namespace Foxtaur.LibRenderer.Services.Abstractions.Zoom;

public class OnZoomLevelChangedArgs
{
    public ZoomLevel ZoomLevel { get; private set; }

    public OnZoomLevelChangedArgs(ZoomLevel zoomLevel)
    {
        ZoomLevel = zoomLevel;
    }
}

/// <summary>
/// Service to work with zoom, zoom levels and so on
/// </summary>
public interface IZoomService
{
    /// <summary>
    /// Called when zoom level changed
    /// </summary>
    delegate void OnZoomLevelChangedHandler(object sender, OnZoomLevelChangedArgs args);
    
    /// <summary>
    /// Event for zoom level change
    /// </summary>
    event OnZoomLevelChangedHandler OnZoomLevelChanged;

    /// <summary>
    /// Current zoom
    /// </summary>
    double Zoom { get; set; }

    /// <summary>
    /// Surface run mode. Write it here (from renderer). In case of IsSurfaceRunMode == true,
    /// special zoom level(s) will be activated
    /// </summary>
    bool IsSurfaceRunMode { get; set; }

    /// <summary>
    /// Current zoom level
    /// </summary>
    ZoomLevel ZoomLevel { get; }
    
    /// <summary>
    /// Current zoom level data
    /// </summary>
    Models.Zoom.ZoomLevel ZoomLevelData { get; }
}