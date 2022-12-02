using Foxtaur.LibRenderer.Models.Zoom;

namespace Foxtaur.LibRenderer.Services.Abstractions.Zoom;

public class OnZoomLevelChangedArgs
{
    public Enums.ZoomLevel ZoomLevel { get; private set; }

    public OnZoomLevelChangedArgs(Enums.ZoomLevel zoomLevel)
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
    float Zoom { get; set; }

    /// <summary>
    /// Current zoom level
    /// </summary>
    Enums.ZoomLevel ZoomLevel { get; }
    
    /// <summary>
    /// Current zoom level data
    /// </summary>
    ZoomLevel ZoomLevelData { get; }
}