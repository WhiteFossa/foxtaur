using System;
using Foxtaur.LibGeo.Models;
using Foxtaur.LibResources.Models.HighResMap;

namespace Foxtaur.Desktop.Controls.Renderer.Models;

/// <summary>
/// Earth segment with a map image
/// </summary>
public class MapSegment
{
    /// <summary>
    /// Coordinates of segment
    /// </summary>
    public GeoSegment GeoSegment { get; private set; }

    /// <summary>
    /// Mesh for given segment
    /// </summary>
    public Mesh Mesh { get; private set; }

    /// <summary>
    /// Actual map, image is stored here
    /// </summary>
    public HighResMap Map { get; private set; }

    /// <summary>
    /// Segment texture
    /// </summary>
    public Texture Texture { get; private set; }

    /// <summary>
    /// Note - we start WITHOUT texture
    /// </summary>
    public MapSegment(GeoSegment geoSegment, Mesh mesh, HighResMap map)
    {
        GeoSegment = geoSegment ?? throw new ArgumentNullException(nameof(geoSegment));
        Mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        Map = map ?? throw new ArgumentNullException(nameof(map));
    }

    public void UpdateTexture(Texture newTexture)
    {
        if (Texture != null)
        {
            Texture.Dispose();
        }

        Texture = newTexture;
    }
}