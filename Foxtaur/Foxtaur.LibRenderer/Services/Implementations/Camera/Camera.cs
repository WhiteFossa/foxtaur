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
    private PlanarPoint3D _position = new PlanarPoint3D(0, 0, 0);

    /// <summary>
    /// Camera target
    /// </summary>
    private PlanarPoint3D _target = new PlanarPoint3D(0, 0, 0);
    
    /// <summary>
    /// Camera zoom
    /// </summary>
    private float _zoom;

    /// <summary>
    /// Camera direction
    /// </summary>
    private Vector3 _direction = new Vector3(0, 0, 0);

    /// <summary>
    /// Camera up vector
    /// </summary>
    private Vector3 _up;

    /// <summary>
    /// Camera aspect ratio
    /// </summary>
    private float _aspectRatio;

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

        set
        {
            _position = value;
            
            CalculateCameraDirection();
            CalculateMatrices();
        }
    }

    public PlanarPoint3D Target
    {
        get
        {
            return _target;
        }

        set
        {
            _target = value;
            
            CalculateCameraDirection();
            CalculateMatrices();
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
            
            CalculateMatrices();
        }
    }

    public Matrix4x4 ModelMatrix { get; private set; } = new Matrix4x4();

    public Matrix4x4 ViewMatrix { get; private set; } = new Matrix4x4();

    public Matrix4x4 ProjectionMatrix { get; private set; } = new Matrix4x4();

    public Matrix4x4 BackProjectionMatrix { get; private set; } = new Matrix4x4();

    public float AspectRatio
    {
        get
        {
            return _aspectRatio;
        }

        set
        {
            _aspectRatio = value;

            CalculateMatrices();
        }
    }

    private Vector3 Up
    {
        get
        {
            return _up;
        }

        set
        {
            _up = value;
            
            CalculateMatrices();
        }
    }
    
    public Camera(ISphereCoordinatesProvider sphereCoordinatesProvider)
    {
        _sphereCoordinatesProvider = sphereCoordinatesProvider;

        _zoom = RendererConstants.CameraMinZoom; // To avoid errors during initialization

        AspectRatio = 1.0f;
        Up = new Vector3(0.0f, -1.0f, 0.0f);
        Zoom = RendererConstants.CameraMinZoom;
        
        CalculateCameraPosition();
        CalculateMatrices();
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
        Position3D = _sphereCoordinatesProvider.GeoToPlanar3D(new GeoPoint(Lat, Lon, H));
    }

    private void CalculateCameraDirection()
    {
        _direction = Vector3.Normalize(_position.AsVector3() - _target.AsVector3());
    }

    /// <summary>
    /// Call on next changes:
    /// - Position3D
    /// - Target
    /// - Up
    /// - Zoom
    /// - AspectRatio
    /// </summary>
    private void CalculateMatrices()
    {
        var direction = Vector3.Normalize(Position3D.AsVector3() - Target.AsVector3());

        ModelMatrix = Matrix4x4.CreateRotationZ(0.0f) * Matrix4x4.CreateRotationY(0.0f) * Matrix4x4.CreateRotationX(0.0f); // Rotation
        ViewMatrix = Matrix4x4.CreateLookAt(Position3D.AsVector3(), direction, Up); // Camera position
        ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(Zoom, AspectRatio, 0.1f, 100.0f); // Zoom
        
        // Back-projection matrix (for raycasting)
        var forwardProjection = ViewMatrix * ProjectionMatrix;

        Matrix4x4 backProjection;
        if (!Matrix4x4.Invert(forwardProjection, out backProjection))
        {
            throw new InvalidOperationException("Failed to invert forward projection!");
        }

        BackProjectionMatrix = backProjection;
    }
}