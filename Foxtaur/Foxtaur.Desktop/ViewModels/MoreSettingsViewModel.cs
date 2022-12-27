using ReactiveUI;

namespace Foxtaur.Desktop.ViewModels;

/// <summary>
/// View model for "More settings" dialogue
/// </summary>
public class MoreSettingsViewModel : ViewModelBase
{
    #region Bound properties
    
    private int _surfaceRunModeForwardButtonIndex;
    private int _surfaceRunModeBackwardButtonIndex;
    private int _surfaceRunModeTurnLeftButtonIndex;
    private int _surfaceRunModeTurnRightButtonIndex;
    
    /// <summary>
    /// Surface run mode forward button index
    /// </summary>
    public int SurfaceRunModeForwardButtonIndex
    {
        get => _surfaceRunModeForwardButtonIndex;
        set => this.RaiseAndSetIfChanged(ref _surfaceRunModeForwardButtonIndex, value);
    }
    
    /// <summary>
    /// Surface run mode backward button index
    /// </summary>
    public int SurfaceRunModeBackwardButtonIndex
    {
        get => _surfaceRunModeBackwardButtonIndex;
        set => this.RaiseAndSetIfChanged(ref _surfaceRunModeBackwardButtonIndex, value);
    }
    
    /// <summary>
    /// Surface run mode turn left button index
    /// </summary>
    public int SurfaceRunModeTurnLeftButtonIndex
    {
        get => _surfaceRunModeTurnLeftButtonIndex;
        set => this.RaiseAndSetIfChanged(ref _surfaceRunModeTurnLeftButtonIndex, value);
    }
    
    /// <summary>
    /// Surface run mode turn right button index
    /// </summary>
    public int SurfaceRunModeTurnRightButtonIndex
    {
        get => _surfaceRunModeTurnRightButtonIndex;
        set => this.RaiseAndSetIfChanged(ref _surfaceRunModeTurnRightButtonIndex, value);
    }
    
    #endregion
    
}