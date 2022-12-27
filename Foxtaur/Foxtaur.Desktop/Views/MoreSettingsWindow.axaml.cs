using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Foxtaur.Desktop.Views;

public partial class MoreSettingsWindow : Window
{
    /// <summary>
    /// Keys, available to select as keyboard controls
    /// </summary>
    private readonly List<Key> AvailableKeys = new List<Key>()
    {
        Key.A,
        Key.B,
        Key.C,
        Key.D,
        Key.E,
        Key.F,
        Key.G,
        Key.H,
        Key.I,
        Key.J,
        Key.K,
        Key.L,
        Key.M,
        Key.N,
        Key.O,
        Key.P,
        Key.Q,
        Key.R,
        Key.S,
        Key.T,
        Key.U,
        Key.V,
        Key.W,
        Key.X,
        Key.Y,
        Key.Z
    };
    
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
        surfaceRunModeForwardButtonCombobox.Items = AvailableKeys;
        
        // Back
        var surfaceRunModeBackButtonCombobox = this.Find<ComboBox>("surfaceRunBackButton");
        surfaceRunModeBackButtonCombobox.Items = AvailableKeys;
        
        // Left
        var surfaceRunModeTurnLeftButtonCombobox = this.Find<ComboBox>("surfaceRunTurnLeftButton");
        surfaceRunModeTurnLeftButtonCombobox.Items = AvailableKeys;
        
        // Right
        var surfaceRunModeTurnRightButtonCombobox = this.Find<ComboBox>("surfaceRunTurnRightButton");
        surfaceRunModeTurnRightButtonCombobox.Items = AvailableKeys;
    }
}