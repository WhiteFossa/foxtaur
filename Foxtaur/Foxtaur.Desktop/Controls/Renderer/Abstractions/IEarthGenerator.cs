namespace Foxtaur.Desktop.Controls.Renderer.Abstractions;

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
}