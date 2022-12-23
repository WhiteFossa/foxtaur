using System;
using ReactiveUI;

namespace Foxtaur.Desktop.ViewModels
{
    /// <summary>
    ///     Main view model
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        #region Bound properties

        private string _consoleText;
        private int _consoleCaretIndex;
        private double _demScale;
        private string _demScaleText;

        /// <summary>
        /// Text in console
        /// </summary>
        public string ConsoleText
        {
            get => _consoleText;
            set => this.RaiseAndSetIfChanged(ref _consoleText, value);
        }

        /// <summary>
        /// Consone caret index (to scroll programmatically)
        /// </summary>
        public int ConsoleCaretIndex
        {
            get => _consoleCaretIndex;
            set => this.RaiseAndSetIfChanged(ref _consoleCaretIndex, value);
        }

        /// <summary>
        /// DEM scaling factor
        /// </summary>
        public double DemScale
        {
            get => _demScale;
            set
            {
                this.RaiseAndSetIfChanged(ref _demScale, value);
                DemScaleText = $"{value:0.#}";
            }
        }

        /// <summary>
        /// DEM scale as text
        /// </summary>
        public string DemScaleText
        {
            get => _demScaleText;
            set => this.RaiseAndSetIfChanged(ref _demScaleText, value);
        }
        
        #endregion

        public MainWindowViewModel()
        {
            // Loading settings (TODO: Load from DB)
            DemScale = 1.0;
        }

        #region Logging

        /// <summary>
        /// Adds a new text line to console. Feed it to logger
        /// </summary>
        public void AddLineToConsole(string line)
        {
            ConsoleText += $"{line}{Environment.NewLine}";

            ConsoleCaretIndex = ConsoleText.Length;
        }

        #endregion
    }
}