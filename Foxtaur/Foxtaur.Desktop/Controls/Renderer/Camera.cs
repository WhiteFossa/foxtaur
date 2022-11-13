namespace Foxtaur.Desktop.Controls.Renderer;

/// <summary>
/// Camera
/// </summary>
public class Camera
{
    /// <summary>
    /// Camera latitude
    /// </summary>
    public float Lat { get; set; }
    
    /// <summary>
    /// Camera longitude
    /// </summary>
    public float Lon { get; set; }

    /// <summary>
    /// Camera height (can't be less than Earth radius)
    /// </summary>
    public float H { get; set; }
}