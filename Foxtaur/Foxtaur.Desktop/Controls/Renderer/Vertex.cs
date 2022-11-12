using System.Numerics;
using System.Runtime.InteropServices;

namespace Foxtaur.Desktop.Controls.Renderer;

/// <summary>
/// One vertex
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Vertex
{
    /// <summary>
    /// Vertex position
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// Texture coordinates
    /// </summary>
    public Vector2 TextureCoords;
}