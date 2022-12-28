using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Foxtaur.LibSettings.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Foxtaur.Desktop.Views;

public partial class MoreSettingsWindow : Window
{
    private readonly ISettingsService _settingsService = Program.Di.GetService<ISettingsService>();
    
    public MoreSettingsWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        
        // Controls comboboxes content
        
        // Forward
        var surfaceRunModeForwardButtonCombobox = this.Find<ComboBox>("surfaceRunForwardButton");
        surfaceRunModeForwardButtonCombobox.Items = _settingsService.GetKeyboardKeysCollection();
        
        // Back
        var surfaceRunModeBackButtonCombobox = this.Find<ComboBox>("surfaceRunBackButton");
        surfaceRunModeBackButtonCombobox.Items = _settingsService.GetKeyboardKeysCollection();
        
        // Left
        var surfaceRunModeTurnLeftButtonCombobox = this.Find<ComboBox>("surfaceRunTurnLeftButton");
        surfaceRunModeTurnLeftButtonCombobox.Items = _settingsService.GetKeyboardKeysCollection();
        
        // Right
        var surfaceRunModeTurnRightButtonCombobox = this.Find<ComboBox>("surfaceRunTurnRightButton");
        surfaceRunModeTurnRightButtonCombobox.Items = _settingsService.GetKeyboardKeysCollection();
    }
}