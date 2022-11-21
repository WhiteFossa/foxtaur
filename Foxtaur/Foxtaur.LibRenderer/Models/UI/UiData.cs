namespace Foxtaur.LibRenderer.Models.UI;

/// <summary>
/// Data for UI
/// </summary>
public class UiData
{
    /// <summary>
    /// If true, then GUI have to be regenerated
    /// </summary>
    public bool IsRegenerationRequested { get; private set; }

    /// <summary>
    /// FPS
    /// </summary>
    public float Fps { get; set; }

    /// <summary>
    /// Is mouse in Earth (and so have geocoordinates)?
    /// </summary>
    public bool IsMouseInEarth { get; set; }

    /// <summary>
    /// Mouse latitude
    /// </summary>
    public float MouseLat { get; set; }

    /// <summary>
    /// Mouse longitude
    /// </summary>
    public float MouseLon { get; set; }

    /// <summary>
    /// Mark UI for regeneration
    /// </summary>
    public void MarkForRegeneration()
    {
        IsRegenerationRequested = true;
    }

    /// <summary>
    /// Mark UI as regenerated
    /// </summary>
    public void MarkAsRegenerated()
    {
        IsRegenerationRequested = false;
    }
}