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
    public double Fps { get; set; }

    /// <summary>
    /// Is mouse in Earth (and so have geocoordinates)?
    /// </summary>
    public bool IsMouseInEarth { get; set; }

    /// <summary>
    /// Mouse latitude
    /// </summary>
    public double MouseLat { get; set; }

    /// <summary>
    /// Mouse longitude
    /// </summary>
    public double MouseLon { get; set; }

    /// <summary>
    /// Mouse altitude
    /// </summary>
    public double MouseH { get; set; }

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