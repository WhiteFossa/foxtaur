using Foxtaur.LibRenderer.Constants;
using Foxtaur.LibRenderer.Models;
using Foxtaur.LibRenderer.Services.Abstractions.CoordinateProviders;
using Foxtaur.LibRenderer.Services.Implementations.CoordinateProviders;
using System;
using System.Numerics;

namespace Foxtaur.Desktop.Controls.Renderer;

/// <summary>
///     Camera
/// </summary>
public class Camera
{
    /// <summary>
    /// Camera latitude
    /// </summary>
    public float Lat { get; set; }

    /// <summary>
    /// Camera longitude
    /// </summary>
    public float Lon { get; set; }

    /// <summary>
    /// Camera height (can't be less than Earth radius)
    /// </summary>
    public float H
    {
        get { return _h; }

        set
        {
            if (value < RendererConstants.EarthRadius - RendererConstants.PlanarCoordinatesCheckDelta)
            {
                throw new ArgumentException(nameof(value));
            }

            _h = value;
        }
    }

    /// <summary>
    /// Camera height
    /// </summary>
    private float _h;

    /// <summary>
    /// Camera zoom
    /// </summary>
    private float _zoom;

    /// <summary>
    /// Camera zoom (with overzoom and underzoom protection)
    /// </summary>
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

    private ICoordinatesProvider _sphereCoordinatesProvider = new SphereCoordinatesProvider();

    /// <summary>
    /// Get camera position
    /// </summary>
    public PlanarPoint3D GetCameraPosition()
    {
        return _sphereCoordinatesProvider.GeoToPlanar3D(new GeoPoint(Lat, Lon, H));
    }

    /// <summary>
    /// Get camera position
    /// </summary>
    public Vector3 GetCameraPositionAsVector()
    {
        PlanarPoint3D position = GetCameraPosition();
        return new Vector3(position.X, position.Y, position.Z);
    }

    /// <summary>
    /// Zoom in
    /// </summary>
    public void ZoomIn(float steps)
    {
        Zoom = Zoom * (float)Math.Pow(RendererConstants.CameraZoomInMultiplier, steps);
    }

    /// <summary>
    /// Zoom out
    /// </summary>
    public void ZoomOut(float steps)
    {
        Zoom = Zoom * (float)Math.Pow(RendererConstants.CameraZoomOutMultiplier, steps);
    }
}