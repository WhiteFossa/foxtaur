using System;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer;

/// <summary>
///     Vertex array object
/// </summary>
public class VertexArrayObject<TVertexType, TIndexType> : IDisposable
    where TVertexType : unmanaged
    where TIndexType : unmanaged
{
    private uint _handle;
    private GL _silkGl;

    public VertexArrayObject(GL silkGl, BufferObject<TVertexType> verticesBufferObject,
        BufferObject<TIndexType> elementsBufferObject)
    {
        _silkGl = silkGl ?? throw new ArgumentNullException(nameof(silkGl));
        _ = verticesBufferObject ?? throw new ArgumentNullException(nameof(verticesBufferObject));
        _ = elementsBufferObject ?? throw new ArgumentNullException(nameof(elementsBufferObject));

        _handle = _silkGl.GenVertexArray();
        Bind();
        verticesBufferObject.Bind();
        elementsBufferObject.Bind();
    }

    public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint vertexSize,
        int offSet)
    {
        _silkGl.VertexAttribPointer(index, count, type, false, vertexSize * (uint)sizeof(TVertexType),
            (void*)(offSet * sizeof(TVertexType)));
        _silkGl.EnableVertexAttribArray(index);
    }

    public void Bind()
    {
        _silkGl.BindVertexArray(_handle);
    }

    public void Dispose()
    {
        _silkGl.DeleteVertexArray(_handle);
    }
}