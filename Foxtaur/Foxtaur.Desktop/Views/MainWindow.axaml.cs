using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Foxtaur.Desktop.ViewModels;

namespace Foxtaur.Desktop.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            this.EarthRenderer.OnKeyPressed(e);
        }
    }
}