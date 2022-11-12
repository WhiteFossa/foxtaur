using Foxtaur.LibRenderer.Models;

namespace Foxtaur.LibRenderer.Services.Abstractions;

/// <summary>
/// Textures loader
/// </summary>
public interface ITexturesLoader
{
    /// <summary>
    /// Load texture from file (image actually)
    /// </summary>
    Texture LoadTextureFromFile(string path);
}