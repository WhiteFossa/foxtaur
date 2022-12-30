using System.Collections.Generic;
using System.Linq;
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
        
        public MainWindow(MainWindowViewModel dataContext)
        {
            InitializeComponent();

            DataContext = dataContext;
            
            var distancesComboBox = this.Find<ComboBox>("Distances");
            distancesComboBox.Items = ((MainWindowViewModel)DataContext)
                .GetDistances()
                .Select(d => d.Name);

            ((MainWindowViewModel)DataContext).Renderer = EarthRenderer;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            EarthRenderer.OnKeyPressed(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            
            EarthRenderer.OnKeyReleased(e);
        }
    }
}