using System.Numerics;
using Foxtaur.LibRenderer.Constants;
using Foxtaur.LibRenderer.Helpers;
using Foxtaur.LibRenderer.Models;
using Foxtaur.LibRenderer.Services.Abstractions.Camera;
using Foxtaur.LibRenderer.Services.Abstractions.CoordinateProviders;
using Foxtaur.LibRenderer.Services.Implementations.CoordinateProviders;

namespace Foxtaur.LibRenderer.Services.Implementations.Camera;

public class Camera : ICamera
{
    private ICoordinatesProvider _sphereCoordinatesProvider = new SphereCoordinatesProvider();

    /// <summary>
    /// Camera latitude
    /// </summary>
    private float _lat;

    /// <summary>
    /// Camera longitude
    /// </summary>
    private float _lon;
    
    /// <summary>
    /// Camera height
    /// </summary>
    private float _h;

    /// <summary>
    /// Camera position in 3D space
    /// </summary>
    private PlanarPoint3D _position;
    
    /// <summary>
    /// Camera zoom
    /// </summary>
    private float _zoom;

    public float Lat
    {
        get
        {
            return _lat;
        }

        set
        {
            _lat = value;
            
            CalculateCameraPosition();
        }
    }
    
    public float Lon
    {
        get
        {
            return _lon;
        }

        set
        {
            _lon = value;
            
            CalculateCameraPosition();
        }
    }

    public float H
    {
        get { return _h; }

        set
        {
            if (value < RendererConstants.EarthRadius)
            {
                _h = RendererConstants.EarthRadius;
            }

            _h = value;
            
            CalculateCameraPosition();
        }
    }

    public PlanarPoint3D Position3D
    {
        get
        {
            return _position;
        }
    }

    public float Zoom
    {
        get
        {
            return _zoom;
        }

        set
        {
            if (value < RendererConstants.CameraMaxZoom)
            {
                _zoom = RendererConstants.CameraMaxZoom;
            }
            else if (value > RendererConstants.CameraMinZoom)
            {
                _zoom = RendererConstants.CameraMinZoom;
            }
            else
            {
                _zoom = value;
            }
        }
    }

    public Camera(ISphereCoordinatesProvider sphereCoordinatesProvider)
    {
        _sphereCoordinatesProvider = sphereCoordinatesProvider;
    }

    public void ZoomIn(float steps)
    {
        Zoom = Zoom * (float)Math.Pow(RendererConstants.CameraZoomInMultiplier, steps);
    }

    public void ZoomOut(float steps)
    {
        Zoom = Zoom * (float)Math.Pow(RendererConstants.CameraZoomOutMultiplier, steps);
    }

    private void CalculateCameraPosition()
    {
        _position = _sphereCoordinatesProvider.GeoToPlanar3D(new GeoPoint(Lat, Lon, H));
    }
}