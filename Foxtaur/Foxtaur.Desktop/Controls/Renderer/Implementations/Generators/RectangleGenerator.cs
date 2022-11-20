using Foxtaur.Desktop.Controls.Renderer.Abstractions.Generators;
using Foxtaur.LibRenderer.Models;

namespace Foxtaur.Desktop.Controls.Renderer.Implementations.Generators;

public class RectangleGenerator : IRectangleGenerator
{
    public Mesh GenerateRectangle(PlanarPoint3D p1, PlanarPoint2D t1, PlanarPoint3D p2, PlanarPoint2D t2)
    {
        var result = new Mesh();
        
        // P1----P3
        // |     |
        // P4----P2

        var p3 = new PlanarPoint3D(p2.X, p1.Y, p2.Z);
        var t3 = new PlanarPoint2D(t2.X, t1.Y);

        var p4 = new PlanarPoint3D(p1.X, p2.Y, p1.Z);
        var t4 = new PlanarPoint2D(t1.X, t2.Y);
        
        var i1 = result.AddVertex(p1, t1);
        var i2 = result.AddVertex(p2, t2);
        var i3 = result.AddVertex(p3, t3);
        var i4 = result.AddVertex(p4, t4);
        
        result.AddIndex(i4);
        result.AddIndex(i1);
        result.AddIndex(i3);
        
        result.AddIndex(i3);
        result.AddIndex(i2);
        result.AddIndex(i4);

        return result;
    }
}