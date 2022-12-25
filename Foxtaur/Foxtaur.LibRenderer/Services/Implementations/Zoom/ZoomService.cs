using Foxtaur.Helpers;
using Foxtaur.LibRenderer.Constants;
using Foxtaur.LibRenderer.Services.Abstractions.Zoom;
using Foxtaur.LibResources.Enums;

namespace Foxtaur.LibRenderer.Services.Implementations.Zoom;

public class ZoomService : IZoomService
{
    /// <summary>
    /// Zoom level changed event
    /// </summary>
    public event IZoomService.OnZoomLevelChangedHandler? OnZoomLevelChanged;
    
    /// <summary>
    /// Current zoom
    /// </summary>
    public double Zoom
    {
        get
        {
            return _zoom;
        }
        
        set
        {
            _zoom = value;

            ZoomLevel = GetZoomLevelByCameraZoom(_zoom);
        }
    }

    /// <summary>
    /// Current zoom level
    /// </summary>
    public ZoomLevel ZoomLevel
    {
        get
        {
            return _zoomLevel;
        }

        private set
        {
            if (value != _zoomLevel)
            {
                _zoomLevel = value;
                ZoomLevelData = GetZoomLevel(_zoomLevel);
                
                OnZoomLevelChanged?.Invoke(this, new OnZoomLevelChangedArgs(_zoomLevel));
            }
        }
    }

    public Models.Zoom.ZoomLevel ZoomLevelData
    {
        get
        {
            return _zoomLevelData;
        }

        private set
        {
            _zoomLevelData = value;
        }
    }

    private double _zoom = RendererConstants.CameraMinZoom;
    private ZoomLevel _zoomLevel;
    private Models.Zoom.ZoomLevel _zoomLevelData;

    private List<Models.Zoom.ZoomLevel> _zoomLevels = new()
    {
        new(ZoomLevel.ZoomLevel0,
            Math.PI / 2.0f,
            0.5,
            10.0.ToRadians(),
            0.5.ToRadians()),
        
        new(ZoomLevel.ZoomLevel1,
            0.5,
            0.1,
            2.0.ToRadians(),
            0.1.ToRadians()),
        
        new(ZoomLevel.ZoomLevel2,
            0.1,
            0.0,
            1.0.ToRadians(),
            0.05.ToRadians())
    };

    public ZoomService()
    {
        ZoomLevel = GetZoomLevelByCameraZoom(Zoom);
        ZoomLevelData = GetZoomLevel(ZoomLevel);
    }
    
    private ZoomLevel GetZoomLevelByCameraZoom(double cameraZoom)
    {
        return _zoomLevels
            .First(zl => zl.MinZoom >= cameraZoom && zl.MaxZoom < cameraZoom)
            .Level;
    }

    private Models.Zoom.ZoomLevel GetZoomLevel(ZoomLevel zoomLevel)
    {
        return _zoomLevels.First(zl => zl.Level == zoomLevel);
    }
}