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