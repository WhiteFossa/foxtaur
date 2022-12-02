using Foxtaur.Helpers;
using Foxtaur.LibRenderer.Constants;
using Foxtaur.LibRenderer.Enums;
using Foxtaur.LibRenderer.Services.Abstractions.Zoom;

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
    public float Zoom
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

    private float _zoom = RendererConstants.CameraMinZoom;
    private ZoomLevel _zoomLevel;
    private Models.Zoom.ZoomLevel _zoomLevelData;

    private List<Models.Zoom.ZoomLevel> _zoomLevels = new()
    {
        new(ZoomLevel.ZoomLevel0,
            (float)Math.PI / 2.0f,
            0.3f,
            10.0f.ToRadians(),
            20.0f),
        
        new(ZoomLevel.ZoomLevel1,
            0.3f,
            0.1f,
            2.0f.ToRadians(),
            50.0f),
        
        new(ZoomLevel.ZoomLevel2,
            0.1f,
            0.0f,
            1.0f.ToRadians(),
            500.0f)
    };

    public ZoomService()
    {
        ZoomLevel = GetZoomLevelByCameraZoom(Zoom);
        ZoomLevelData = GetZoomLevel(ZoomLevel);
    }
    
    private ZoomLevel GetZoomLevelByCameraZoom(float cameraZoom)
    {
        return _zoomLevels
            .First(zl => zl.MinZoom >= cameraZoom && zl.MaxZoom < cameraZoom)
            .Level;
    }

    private Models.Zoom.ZoomLevel GetZoomLevel(ZoomLevel zoomLevel)
    {
        return _zoomLevels
            .First(zl => zl.Level == zoomLevel);
    }
}