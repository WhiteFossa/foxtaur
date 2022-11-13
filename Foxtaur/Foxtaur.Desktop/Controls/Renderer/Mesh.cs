using System;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer;

/// <summary>
/// Displayeable mesh
/// </summary>
public class Mesh: IDisposable
{
    /// <summary>
    /// Vertex coordinates size (in floats)
    /// </summary>
    private const int VertexCoordsSize = 3;

    /// <summary>
    /// Texture coordinates size (in floats)
    /// </summary>
    private const int TextureCoordsSize = 2;
    
    /// <summary>
    /// Vertices positions
    /// </summary>
    private const int VerticesPositionLocation = 0;

    /// <summary>
    /// Vertices texture coords
    /// </summary>
    private const int VerticesTextureCoordsLocation = 1;
    
    /// <summary>
    /// Vertex size (in floats)
    /// </summary>
    private const int VertexSize = VertexCoordsSize + TextureCoordsSize;

    /// <summary>
    /// Vertices
    /// </summary>
    public float[] Vertices;

    /// <summary>
    /// Indices
    /// </summary>
    public uint[] Indices;

    /// <summary>
    /// Vertices buffer object
    /// </summary>
    public BufferObject<float> VerticesBufferObject { get; private set; }

    /// <summary>
    /// Elements (indices) buffer object
    /// </summary>
    public BufferObject<uint> ElementsBufferObject { get; private set; }

    /// <summary>
    /// Vertices array object
    /// </summary>
    public VertexArrayObject<float, uint> VerticesArrayObject { get; private set; }

    /// <summary>
    /// Generate OpenGL buffers for mesh
    /// </summary>
    public void GenerateBuffers(GL silkGl)
    {
        _ = silkGl ?? throw new ArgumentNullException(nameof(silkGl));
        
        VerticesBufferObject = new BufferObject<float>(silkGl, Vertices, BufferTargetARB.ArrayBuffer);

        // Object for indices
        ElementsBufferObject = new BufferObject<uint>(silkGl, Indices, BufferTargetARB.ElementArrayBuffer);

        // Vertices array
        VerticesArrayObject = new VertexArrayObject<float, uint>(silkGl, VerticesBufferObject, ElementsBufferObject);

        //Telling the VAO object how to lay out the attribute pointers
        VerticesArrayObject.VertexAttributePointer(VerticesPositionLocation, VertexCoordsSize, VertexAttribPointerType.Float, VertexSize, 0);
        VerticesArrayObject.VertexAttributePointer(VerticesTextureCoordsLocation, TextureCoordsSize, VertexAttribPointerType.Float, VertexSize, VertexCoordsSize);
    }

    /// <summary>
    /// Bind buffers (into GL context)
    /// </summary>
    public void BindBuffers()
    {
        ElementsBufferObject.Bind();
        VerticesBufferObject.Bind();
        VerticesArrayObject.Bind();
    }
    
    public void Dispose()
    {
        VerticesBufferObject.Dispose();
        ElementsBufferObject.Dispose();
        VerticesArrayObject.Dispose();
    }
}