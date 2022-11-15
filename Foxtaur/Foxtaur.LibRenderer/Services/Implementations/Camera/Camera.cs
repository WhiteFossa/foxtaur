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
    /// Camera height
    /// </summary>
    private float _h;

    /// <summary>
    /// Camera zoom
    /// </summary>
    private float _zoom;
    
    public float Lat { get; set; }
    
    public float Lon { get; set; }

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
            if (value < RendererConstants.CameraMinZoom)
            {
                _zoom = RendererConstants.CameraMinZoom;
            }
            else if (value > RendererConstants.CameraMaxZoom)
            {
                _zoom = RendererConstants.CameraMaxZoom;
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

    public PlanarPoint3D GetCameraPosition()
    {
        return _sphereCoordinatesProvider.GeoToPlanar3D(new GeoPoint(Lat, Lon, H));
    }

    public Vector3 GetCameraPositionAsVector()
    {
        return GetCameraPosition().AsVector3();
    }

    public void ZoomIn(float steps)
    {
        Zoom = Zoom * (float)Math.Pow(RendererConstants.CameraZoomInMultiplier, steps);
    }

    public void ZoomOut(float steps)
    {
        Zoom = Zoom * (float)Math.Pow(RendererConstants.CameraZoomOutMultiplier, steps);
    }
}