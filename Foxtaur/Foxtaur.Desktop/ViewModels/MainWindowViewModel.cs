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

        #endregion

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