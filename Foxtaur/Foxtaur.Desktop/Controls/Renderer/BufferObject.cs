using System;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer;

/// <summary>
///     OpenGL buffer
/// </summary>
public class BufferObject<TDataType> : IDisposable where TDataType : unmanaged
{
    private uint _handle;
    private BufferTargetARB _bufferType;
    private GL _silkGl;

    public unsafe BufferObject(GL silkGl, Span<TDataType> data, BufferTargetARB bufferType)
    {
        _silkGl = silkGl ?? throw new ArgumentNullException(nameof(silkGl));
        _bufferType = bufferType;

        _handle = _silkGl.GenBuffer();
        Bind();
        fixed (void* d = data)
        {
            _silkGl.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
        }
    }

    public void Bind()
    {
        _silkGl.BindBuffer(_bufferType, _handle);
    }

    public void Dispose()
    {
        _silkGl.DeleteBuffer(_handle);
    }
}