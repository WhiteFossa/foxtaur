using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Input;
using Foxtaur.LibSettings.Services.Abstractions;
using ReactiveUI;
using Microsoft.Extensions.DependencyInjection;

namespace Foxtaur.Desktop.ViewModels;

/// <summary>
/// View model for "More settings" dialogue
/// </summary>
public class MoreSettingsViewModel : ViewModelBase
{
    #region DI

    private readonly ISettingsService _settingsService = Program.Di.GetService<ISettingsService>();
    
    #endregion

    /// <summary>
    /// Settings, which accepts keys
    /// </summary>
    private readonly List<PropertyInfo> _keyProperties;
    
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
        set
        {
            this.RaiseAndSetIfChanged(ref _surfaceRunModeForwardButtonIndex, value);

            if (value != -1)
            {
                RemoveDuplicateKeys("SurfaceRunModeForwardButtonIndex", _settingsService.GetKeyboardKey(value));
            }
        }
    }
    
    /// <summary>
    /// Surface run mode backward button index
    /// </summary>
    public int SurfaceRunModeBackwardButtonIndex
    {
        get => _surfaceRunModeBackwardButtonIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _surfaceRunModeBackwardButtonIndex, value);

            if (value != -1)
            {
                RemoveDuplicateKeys("SurfaceRunModeBackwardButtonIndex", _settingsService.GetKeyboardKey(value));
            }
        }
    }
    
    /// <summary>
    /// Surface run mode turn left button index
    /// </summary>
    public int SurfaceRunModeTurnLeftButtonIndex
    {
        get => _surfaceRunModeTurnLeftButtonIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _surfaceRunModeTurnLeftButtonIndex, value);

            if (value != -1)
            {
                RemoveDuplicateKeys("SurfaceRunModeTurnLeftButtonIndex", _settingsService.GetKeyboardKey(value));
            }
        }
    }
    
    /// <summary>
    /// Surface run mode turn right button index
    /// </summary>
    public int SurfaceRunModeTurnRightButtonIndex
    {
        get => _surfaceRunModeTurnRightButtonIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _surfaceRunModeTurnRightButtonIndex, value);

            if (value != -1)
            {
                RemoveDuplicateKeys("SurfaceRunModeTurnRightButtonIndex", _settingsService.GetKeyboardKey(value));
            }
        }
    }
    
    #endregion

    public MoreSettingsViewModel()
    {
        // Key-accepting properties
        _keyProperties = new List<PropertyInfo>()
        {
            GetType().GetProperty("SurfaceRunModeForwardButtonIndex"),
            GetType().GetProperty("SurfaceRunModeBackwardButtonIndex"),
            GetType().GetProperty("SurfaceRunModeTurnLeftButtonIndex"),
            GetType().GetProperty("SurfaceRunModeTurnRightButtonIndex")
        };
        
        // Commands
        OkPressedCommand = ReactiveCommand.Create<Window>(OnOkPressedCommand);
        CancelPressedCommand = ReactiveCommand.Create<Window>(OnCancelPressedCommand);

        // Loading settings from ISettingsService
        SurfaceRunModeForwardButtonIndex = _settingsService.GetKeyboardKeyIndex(Key.W);
        SurfaceRunModeBackwardButtonIndex = _settingsService.GetKeyboardKeyIndex(Key.S);
        SurfaceRunModeTurnLeftButtonIndex = _settingsService.GetKeyboardKeyIndex(Key.A);
        SurfaceRunModeTurnRightButtonIndex = _settingsService.GetKeyboardKeyIndex(Key.D);
    }
    
    #region Commands
    
    public ReactiveCommand<Window, Unit> OkPressedCommand { get; }
    public ReactiveCommand<Window, Unit> CancelPressedCommand { get; }
    
    private void OnOkPressedCommand(Window window)
    {
        int a = 10;
    }
    
    private void OnCancelPressedCommand(Window window)
    {
        window.Close();
    }

    
    #endregion
    
    /// <summary>
    /// Scan all controls and remove keys, which are duplicate to newly assigned key
    /// </summary>
    private void RemoveDuplicateKeys(string currentPropertyName, Key newlyAssignedKey)
    {
        var propertiesToCheck = _keyProperties
            .Where(kp => !kp.Name.Equals(currentPropertyName, StringComparison.InvariantCulture));

        var newlyAssignedKeyIndex = _settingsService.GetKeyboardKeyIndex(newlyAssignedKey);
        
        var duplicatedPropertines = propertiesToCheck
            .Where(p => (int)p.GetValue(this) == newlyAssignedKeyIndex);
        
        // Resetting duplicates
        foreach (var duplicateProperty in duplicatedPropertines)
        {
            duplicateProperty.SetValue(this, -1);
        }
    }
    
}