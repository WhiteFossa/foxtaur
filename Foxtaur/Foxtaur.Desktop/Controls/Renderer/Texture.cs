using System;
using ImageMagick;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer;

public class Texture : IDisposable
{
    private uint _handle;
    private GL _silkGl;

    public unsafe Texture(GL silkGl, string path)
    {
        _silkGl = silkGl ?? throw new ArgumentNullException(nameof(silkGl));

        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException(nameof(path));
        }

        _handle = this._silkGl.GenTexture();
        Bind();
        
        using (var img = new MagickImage(path))
        {
            fixed (void* pixels = img.GetPixels().ToByteArray(PixelMapping.RGBA))
            {
                _silkGl.TexImage2D(TextureTarget.Texture2D,
                    0,
                    InternalFormat.Rgba8,
                    (uint)img.Width,
                    (uint)img.Height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    pixels);    
            }
        }

        SetParameters();
    }
    
    public unsafe Texture(GL silkGl, MagickImage image)
    {
        _silkGl = silkGl ?? throw new ArgumentNullException(nameof(silkGl));
        
        _handle = this._silkGl.GenTexture();
        Bind();
        
        fixed (void* pixels = image.GetPixels().ToByteArray(PixelMapping.RGBA))
        {
            _silkGl.TexImage2D(TextureTarget.Texture2D,
                0,
                InternalFormat.Rgba8,
                (uint)image.Width,
                (uint)image.Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                pixels);    
        }

        SetParameters();
    }

    public unsafe Texture(GL silkGl, Span<byte> data, uint width, uint height)
    {
        //Saving the gl instance.
        _silkGl = silkGl ?? throw new ArgumentNullException(nameof(silkGl));

        //Generating the opengl handle;
        _handle = _silkGl.GenTexture();
        Bind();

        //We want the ability to create a texture using data generated from code aswell.
        fixed (void* d = &data[0])
        {
            //Setting the data of a texture.
            _silkGl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba,
                PixelType.UnsignedByte, d);
            SetParameters();
        }
    }

    private void SetParameters()
    {
        //Setting some texture perameters so the texture behaves as expected.
        _silkGl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        _silkGl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        _silkGl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)GLEnum.LinearMipmapLinear);
        _silkGl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        _silkGl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
        _silkGl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);

        //Generating mipmaps.
        _silkGl.GenerateMipmap(TextureTarget.Texture2D);
    }

    public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
    {
        //When we bind a texture we can choose which textureslot we can bind it to.
        _silkGl.ActiveTexture(textureSlot);
        _silkGl.BindTexture(TextureTarget.Texture2D, _handle);
    }

    public void Dispose()
    {
        //In order to dispose we need to delete the opengl handle for the texure.
        _silkGl.DeleteTexture(_handle);
    }
}