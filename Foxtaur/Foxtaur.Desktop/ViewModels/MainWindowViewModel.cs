using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Timers;
using Foxtaur.Desktop.Models;
using Foxtaur.Desktop.Views;
using Foxtaur.LibSettings.Services.Abstractions;
using Foxtaur.LibWebClient.Models;
using Foxtaur.LibWebClient.Services.Abstract;
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
        
        private int _selectedDistanceIndex;

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

        /// <summary>
        /// Selected distance index
        /// </summary>
        public int SelectedDistanceIndex
        {
            get => _selectedDistanceIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedDistanceIndex, value);
                
                // Loading full distance data into model
                if (value == -1)
                {
                    _mainModel.Distance = null;
                }
                else
                {
                    _mainModel.Distance = _webClient.GetDistanceByIdAsync(_distances[value].Id).Result;
                }
            }
        }
        
        #endregion

        #region Commands
        
        /// <summary>
        /// Show "More settings" dialogue
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoreSettingsCommand { get; }

        /// <summary>
        /// Show "More settings" dialogue
        /// </summary>
        private void OnMoreSettingsCommand()
        {
            var moreSettingsDialog = new MoreSettingsWindow()
            {
                DataContext = _moreSettingsViewModel
            };

            moreSettingsDialog.ShowDialog(Program.GetMainWindow());
        }
        
        #endregion
        
        #region DI

        private readonly ISettingsService _settingsService = Program.Di.GetService<ISettingsService>();
        private readonly IWebClient _webClient = Program.Di.GetService<IWebClient>();

        #endregion
        
        private Timer _demScaleNotificationTimer = new Timer(1000);
        private MoreSettingsViewModel _moreSettingsViewModel;
        private IList<Distance> _distances;

        private MainModel _mainModel;

        public MainWindowViewModel(MainModel mainModel)
        {
            _mainModel = mainModel ?? throw new ArgumentNullException(nameof(mainModel));
            
            // Binding commands
            MoreSettingsCommand = ReactiveCommand.Create(OnMoreSettingsCommand);
            
            // More settings dialogue
            _moreSettingsViewModel = new MoreSettingsViewModel();
            
            // Loading settings
            DemScale = _settingsService.GetDemScale();
            SurfaceRunSpeed = _settingsService.GetSurfaceRunSpeed();
            SurfaceRunTurnSpeed = _settingsService.GetSurfaceRunTurnSpeed();
            
            _demScaleNotificationTimer.Elapsed += NotifyAboutDemScaleChange;
            _demScaleNotificationTimer.AutoReset = false;
            _demScaleNotificationTimer.Enabled = false;
            
            // Asking for distances
            SelectedDistanceIndex = -1;
            _distances = _webClient.GetDistancesWithoutIncludeAsync()
                .Result
                .ToList();
        }

        /// <summary>
        /// Return distances list
        /// </summary>
        public IList<Distance> GetDistances()
        {
            return _distances;
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