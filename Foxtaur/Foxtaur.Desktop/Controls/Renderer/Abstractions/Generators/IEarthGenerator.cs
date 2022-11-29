using Foxtaur.Desktop.Controls.Renderer.Models;
using Foxtaur.LibGeo.Models;
using Foxtaur.LibRenderer.Models;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer.Abstractions.Generators;

/// <summary>
/// Interface for Earth surface generation
/// </summary>
public interface IEarthGenerator
{
    /// <summary>
    /// Generate full Earth
    /// </summary>
    /// <param name="gridStep">Grid step in radians</param>
    public Mesh GenerateFullEarth(float gridStep);

    /// <summary>
    /// Generate Earth segment
    /// </summary>
    public EarthSegment GenerateEarthSegment(GeoSegment segment, float gridStep);

    /// <summary>
    /// Generate mesh for given Earth segment and store it into segment
    /// </summary>
    public void GenerateMeshForSegment(EarthSegment segment);
    
    /// <summary>
    /// Generate Earth sphere (for raycasting)
    /// </summary>
    public Sphere GenerateEarthSphere();
}