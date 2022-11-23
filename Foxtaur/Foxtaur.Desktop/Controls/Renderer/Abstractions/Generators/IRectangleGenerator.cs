using Foxtaur.LibGeo.Models;

namespace Foxtaur.Desktop.Controls.Renderer.Abstractions.Generators;

/// <summary>
/// Generates rectangle mesh
/// </summary>
public interface IRectangleGenerator
{
    /// <summary>
    /// Generate rectangle mesh by two points (each point have 3D and texture coordinates).
    /// </summary>
    Mesh GenerateRectangle(PlanarPoint3D p1, PlanarPoint2D t1, PlanarPoint3D p2, PlanarPoint2D t2);
}