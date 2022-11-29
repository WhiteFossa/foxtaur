using System;
using Foxtaur.LibGeo.Models;

namespace Foxtaur.Desktop.Controls.Renderer.Models;

/// <summary>
/// Earth segment: geodata + mesh
/// </summary>
public class EarthSegment
{
    /// <summary>
    /// Coordinates of segment
    /// </summary>
    public GeoSegment GeoSegment { get; private set; }

    /// <summary>
    /// Mesh for given segment
    /// </summary>
    public Mesh Mesh { get; private set; }

    public EarthSegment(GeoSegment geoSegment, Mesh mesh)
    {
        GeoSegment = geoSegment ?? throw new ArgumentNullException(nameof(geoSegment));
        Mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
    }
}