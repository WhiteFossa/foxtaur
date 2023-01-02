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

            ZoomLevel = GetZoomLevelByCameraZoom(_zoom, IsSurfaceRunMode);
        }
    }

    public bool IsSurfaceRunMode
    {
        get
        {
            return _isSurfaceRunMode;
        }

        set
        {
            _isSurfaceRunMode = value;

            ZoomLevel = GetZoomLevelByCameraZoom(Zoom, _isSurfaceRunMode);
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
    private bool _isSurfaceRunMode = false;
    private ZoomLevel _zoomLevel;
    private Models.Zoom.ZoomLevel _zoomLevelData;

    private List<Models.Zoom.ZoomLevel> _zoomLevels = new()
    {
        new(ZoomLevel.ZoomLevel0,
            Math.PI / 2.0,
            0.5,
            10.0.ToRadians(),
            0.5.ToRadians()),
        
        new(ZoomLevel.ZoomLevel1,
            0.5,
            0.1,
            5.0.ToRadians(),
            0.1.ToRadians()),
        
        new(ZoomLevel.ZoomLevel2,
            0.1,
            0.0,
            2.0.ToRadians(),
            0.05.ToRadians()),
        
        new(ZoomLevel.ZoomLevelSurfaceRun,
            Math.PI / 2.0,
            0.0,
            2.0.ToRadians(),
            0.002.ToRadians())
    };

    public ZoomService()
    {
        ZoomLevel = GetZoomLevelByCameraZoom(Zoom, IsSurfaceRunMode);
        ZoomLevelData = GetZoomLevel(ZoomLevel);
    }
    
    private ZoomLevel GetZoomLevelByCameraZoom(double cameraZoom, bool isSurfaceRunMode)
    {
        // In case of surface run mode we always want to have higher resolution
        if (isSurfaceRunMode)
        {
            return ZoomLevel.ZoomLevelSurfaceRun;
        }
        
        return _zoomLevels
            .Where(zl => zl.Level != ZoomLevel.ZoomLevelSurfaceRun)
            .First(zl => zl.MinZoom >= cameraZoom && zl.MaxZoom < cameraZoom)
            .Level;
    }

    private Models.Zoom.ZoomLevel GetZoomLevel(ZoomLevel zoomLevel)
    {
        return _zoomLevels.First(zl => zl.Level == zoomLevel);
    }
}