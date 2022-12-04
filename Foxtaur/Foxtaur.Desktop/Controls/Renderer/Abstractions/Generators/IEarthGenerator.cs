using Foxtaur.Desktop.Controls.Renderer.Models;
using Foxtaur.LibGeo.Models;
using Foxtaur.LibRenderer.Models;
using Foxtaur.LibResources.Enums;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer.Abstractions.Generators;

/// <summary>
/// Interface for Earth surface generation
/// </summary>
public interface IEarthGenerator
{
    /// <summary>
    /// Generate Earth segment
    /// </summary>
    public EarthSegment GenerateEarthSegment(GeoSegment segment, float gridStep);

    /// <summary>
    /// Generate mesh for given Earth segment and store it into segment
    /// </summary>
    public void GenerateMeshForSegment(EarthSegment segment, ZoomLevel desiredZoomLevel);
    
    /// <summary>
    /// Generate Earth sphere (for raycasting)
    /// </summary>
    public Sphere GenerateEarthSphere();
}