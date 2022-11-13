using System;
using Foxtaur.LibRenderer.Constants;
using Foxtaur.LibRenderer.Models;
using Foxtaur.LibRenderer.Services.Abstractions.CoordinateProviders;
using Foxtaur.LibRenderer.Services.Implementations.CoordinateProviders;

namespace Foxtaur.Desktop.Controls.Renderer;

/// <summary>
///     Camera
/// </summary>
public class Camera
{
    /// <summary>
    ///     Camera latitude
    /// </summary>
    public float Lat { get; set; }

    /// <summary>
    ///     Camera longitude
    /// </summary>
    public float Lon { get; set; }

    /// <summary>
    ///     Camera height (can't be less than Earth radius)
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
    ///     Camera height
    /// </summary>
    private float _h;

    private ICoordinatesProvider _sphereCoordinatesProvider = new SphereCoordinatesProvider();

    public PlanarPoint3D GetCameraPosition()
    {
        return _sphereCoordinatesProvider.GeoToPlanar3D(new GeoPoint(Lat, Lon, H));
    }
}