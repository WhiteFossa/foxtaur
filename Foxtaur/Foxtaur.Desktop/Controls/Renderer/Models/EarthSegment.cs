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
    /// Segment's mesh step in radians
    /// </summary>
    public float GridStep { get; private set; }

    /// <summary>
    /// Mesh for given segment
    /// </summary>
    public Mesh Mesh { get; private set; }

    /// <summary>
    /// If true, then we need to regenerate mesh for this segment
    /// </summary>
    public bool IsRegenerationNeeded { get; private set; }

    public EarthSegment(GeoSegment geoSegment, float gridStep)
    {
        GeoSegment = geoSegment ?? throw new ArgumentNullException(nameof(geoSegment));
        GridStep = gridStep;
        
        MarkToRegeneration();
    }

    /// <summary>
    /// Mark segment as needing regeneration
    /// </summary>
    public void MarkToRegeneration()
    {
        IsRegenerationNeeded = true;
    }

    /// <summary>
    /// Mark as regenerated
    /// </summary>
    public void MarkAsRegenerated()
    {
        IsRegenerationNeeded = false;
    }

    /// <summary>
    /// Update mesh
    /// </summary>
    public void UpdateMesh(Mesh mesh)
    {
        if (Mesh != null)
        {
            Mesh.Dispose();
        }
        
        Mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
    }
}