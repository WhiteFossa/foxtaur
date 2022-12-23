using System;
using Foxtaur.LibSettings.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
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
                DemScaleText = $"{_demScale:0.#}";
                
                // Notifying
                _settingsService.SetDemScale(_demScale);
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

        #region DI

        private readonly ISettingsService _settingsService = Program.Di.GetService<ISettingsService>();

        #endregion
        
        public MainWindowViewModel()
        {
            // Loading settings
            DemScale = _settingsService.GetDemScale();
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