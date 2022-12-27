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
    }
}