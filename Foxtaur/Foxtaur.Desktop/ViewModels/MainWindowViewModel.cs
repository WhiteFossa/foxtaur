using System;
using System.Timers;
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

        private double _surfaceRunSpeed;
        private string _surfaceRunSpeedText;

        private double _surfaceRunTurnSpeed;
        private string _surfaceRunTurnSpeedText;
        
        private Timer _demScaleNotificationTimer = new Timer(1000);

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
                
                // Resetting notification timer
                _demScaleNotificationTimer.Stop(); // To reset the timer
                _demScaleNotificationTimer.Start();
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

        /// <summary>
        /// Surface run speed
        /// </summary>
        public double SurfaceRunSpeed
        {
            get => _surfaceRunSpeed;
            set
            {
                this.RaiseAndSetIfChanged(ref _surfaceRunSpeed, value);
                SurfaceRunSpeedText = $"{_surfaceRunSpeed:0.#########}";
                
                _settingsService.SetSurfaceRunSpeed(_surfaceRunSpeed);
            }
        }

        /// <summary>
        /// Surface run speed as text
        /// </summary>
        public string SurfaceRunSpeedText
        {
            get => _surfaceRunSpeedText;
            set => this.RaiseAndSetIfChanged(ref _surfaceRunSpeedText, value);
        }

        /// <summary>
        /// Surface run turn speed
        /// </summary>
        public double SurfaceRunTurnSpeed
        {
            get => _surfaceRunTurnSpeed;
            set
            {
                this.RaiseAndSetIfChanged(ref _surfaceRunTurnSpeed, value);
                SurfaceRunTurnSpeedText = $"{_surfaceRunTurnSpeed:0.##°}";
                
                _settingsService.SetSurfaceRunTurnSpeed(_surfaceRunTurnSpeed);
            }
        }

        /// <summary>
        /// Surface run turn speed as text
        /// </summary>
        public string SurfaceRunTurnSpeedText
        {
            get => _surfaceRunTurnSpeedText;
            set => this.RaiseAndSetIfChanged(ref _surfaceRunTurnSpeedText, value);
        }
        
        #endregion

        #region DI

        private readonly ISettingsService _settingsService = Program.Di.GetService<ISettingsService>();

        #endregion

        public MainWindowViewModel()
        {
            // Loading settings
            DemScale = _settingsService.GetDemScale();
            SurfaceRunSpeed = _settingsService.GetSurfaceRunSpeed();
            SurfaceRunTurnSpeed = _settingsService.GetSurfaceRunTurnSpeed();
            
            _demScaleNotificationTimer.Elapsed += NotifyAboutDemScaleChange;
            _demScaleNotificationTimer.AutoReset = false;
            _demScaleNotificationTimer.Enabled = false;
        }

        private void NotifyAboutDemScaleChange(object sender, ElapsedEventArgs e)
        {
            // Notifying
            _settingsService.SetDemScale(_demScale);
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