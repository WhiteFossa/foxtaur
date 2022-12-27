using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Foxtaur.Desktop.Views;

public partial class MoreSettingsWindow : Window
{
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
        surfaceRunModeForwardButtonCombobox.Items = new List<char>() { 'A', 'S', 'D', 'F' };
        
        // Back
        var surfaceRunModeBackButtonCombobox = this.Find<ComboBox>("surfaceRunBackButton");
        surfaceRunModeBackButtonCombobox.Items = new List<char>() { 'A', 'S', 'D', 'F' };
        
        // Left
        var surfaceRunModeLeftButtonCombobox = this.Find<ComboBox>("surfaceRunLeftButton");
        surfaceRunModeLeftButtonCombobox.Items = new List<char>() { 'A', 'S', 'D', 'F' };
        
        // Right
        var surfaceRunModeRightButtonCombobox = this.Find<ComboBox>("surfaceRunRightButton");
        surfaceRunModeRightButtonCombobox.Items = new List<char>() { 'A', 'S', 'D', 'F' };
    }
}