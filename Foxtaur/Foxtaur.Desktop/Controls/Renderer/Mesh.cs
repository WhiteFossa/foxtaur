using System;
using System.Collections.Generic;
using Foxtaur.LibGeo.Models;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer;

/// <summary>
/// Displayeable mesh
/// </summary>
public class Mesh : IDisposable
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
    public List<float> Vertices { get; private set; } = new List<float>();

    /// <summary>
    /// Indices
    /// </summary>
    public List<uint> Indices { get; private set; } = new List<uint>();

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

        VerticesBufferObject = new BufferObject<float>(silkGl, Vertices.ToArray(), BufferTargetARB.ArrayBuffer);

        // Object for indices
        ElementsBufferObject = new BufferObject<uint>(silkGl, Indices.ToArray(), BufferTargetARB.ElementArrayBuffer);
    }

    /// <summary>
    /// Bind buffers (into GL context)
    /// </summary>
    public void BindBuffers(GL silkGl)
    {
        _ = silkGl ?? throw new ArgumentNullException(nameof(silkGl));

        // Vertices array
        VerticesArrayObject = new VertexArrayObject<float, uint>(silkGl, VerticesBufferObject, ElementsBufferObject);

        //Telling the VAO object how to lay out the attribute pointers
        VerticesArrayObject.VertexAttributePointer(VerticesPositionLocation, VertexCoordsSize, VertexAttribPointerType.Float, VertexSize, 0);
        VerticesArrayObject.VertexAttributePointer(VerticesTextureCoordsLocation, TextureCoordsSize, VertexAttribPointerType.Float, VertexSize, VertexCoordsSize);

        VerticesBufferObject.Bind();
        ElementsBufferObject.Bind();
        VerticesArrayObject.Bind();
    }

    /// <summary>
    /// Adds vertex to mesh. Returns index of fresh-added vertex in mesh
    /// </summary>
    public uint AddVertex(PlanarPoint3D vertexCoords, PlanarPoint2D textureCoords)
    {
        _ = vertexCoords ?? throw new ArgumentNullException(nameof(vertexCoords));
        _ = textureCoords ?? throw new ArgumentNullException(nameof(textureCoords));

        Vertices.Add((float)vertexCoords.X);
        Vertices.Add((float)vertexCoords.Y);
        Vertices.Add((float)vertexCoords.Z);
        Vertices.Add((float)textureCoords.X);
        Vertices.Add((float)textureCoords.Y);

        return (uint)(Vertices.Count / VertexSize - 1);
    }

    /// <summary>
    /// Adds index to mesh
    /// </summary>
    public void AddIndex(uint index)
    {
        Indices.Add(index);
    }

    public void Dispose()
    {
        if (VerticesBufferObject != null)
        {
            VerticesBufferObject.Dispose();    
        }

        if (ElementsBufferObject != null)
        {
            ElementsBufferObject.Dispose();    
        }

        if (VerticesArrayObject != null) // BindBuffer() may be never called
        {
            VerticesArrayObject.Dispose();
        }
    }
}