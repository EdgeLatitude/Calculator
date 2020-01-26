﻿using Calculator.Shared.Constants;
using Calculator.Shared.Localization;
using Calculator.Shared.Logic;
using Calculator.Shared.PlatformServices;
using Calculator.Shared.Theming;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Calculator.Shared.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ICommandFactoryService _commandFactoryService;
        private readonly IUiThreadService _uiThreadService;

        private readonly bool _deviceSupportsAutomaticDarkMode;

        private readonly Dictionary<string, Theme?> _themesDictionary = new Dictionary<string, Theme?>
        {
            { LocalizedStrings.Light, Theme.Light },
            { LocalizedStrings.Dark, Theme.Dark }
        };

        private bool _loaded;

        private int _currentHistoryLength;

        private Theme? _currentTheme;

        public SettingsViewModel(
            ICommandFactoryService commandFactoryService,
            IUiThreadService uiThreadService)
        {
            _commandFactoryService = commandFactoryService;
            _uiThreadService = uiThreadService;

            #region History settings
            SaveSettingsCommand = _commandFactoryService.Create(() => SaveSettings(), CanExecuteSaveSettings);
            _currentHistoryLength = Settings.Instance.GetResultsHistoryLength();
            HistoryLength = _currentHistoryLength.ToString();
            #endregion History settings

            #region Theme settings
            DeviceSupportsManualDarkMode = Logic.Theming.Instance.DeviceSupportsManualDarkMode;
            _deviceSupportsAutomaticDarkMode = Logic.Theming.Instance.DeviceSupportsAutomaticDarkMode;
            _currentTheme = Logic.Theming.Instance.GetAppOrDefaultTheme();

            if (_deviceSupportsAutomaticDarkMode)
                _themesDictionary.Add(LocalizedStrings.Device, null);

            _uiThreadService.ExecuteOnUiThread(() =>
            {
                Themes = _themesDictionary.Keys.ToArray();
                SelectedTheme = _themesDictionary.FirstOrDefault(pair => pair.Value == _currentTheme).Key;
                _loaded = true;
            });
            #endregion Theme settings
        }

        private string _historyLength;

        public string HistoryLength
        {
            get => _historyLength;
            set
            {
                if (_historyLength == value)
                    return;
                if (!string.IsNullOrEmpty(value)
                    && !value.All(c => char.IsNumber(c)))
                {
                    HistoryLength = _historyLength;
                    OnPropertyChanged();
                    return;
                }
                _historyLength = value;
                SettingsChanged = true;
                OnPropertyChanged();
            }
        }

        private bool _deviceSupportManualDarkMode;

        public bool DeviceSupportsManualDarkMode
        {
            get => _deviceSupportManualDarkMode;
            set
            {
                if (_deviceSupportManualDarkMode == value)
                    return;
                _deviceSupportManualDarkMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StyleSectionIsVisible));
            }
        }

        private string[] _themes;

        public string[] Themes
        {
            get => _themes;
            set
            {
                if (_themes == value)
                    return;
                _themes = value;
                OnPropertyChanged();
            }
        }

        private string _selectedTheme;

        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (_selectedTheme == value)
                    return;
                _selectedTheme = value;
                SettingsChanged = true;
                OnPropertyChanged();
            }
        }

        private bool _settingsChanged;

        public bool SettingsChanged
        {
            get => _settingsChanged;
            set
            {
                value &= _loaded;
                if (_settingsChanged == value)
                    return;
                _settingsChanged = value;
                SaveSettingsCommand.CanExecute(null);
                OnPropertyChanged();
            }
        }

        public bool StyleSectionIsVisible
        {
            get => DeviceSupportsManualDarkMode;
        }

        public ICommand SaveSettingsCommand { get; private set; }

        private void SaveSettings()
        {
            ManageHistoryLengthSettings();
            ManageThemeSettings();
            SettingsChanged = false;
        }

        private async void ManageHistoryLengthSettings()
        {
            // Try to use the value that the user inputted, else, use the configuration default
            if (!int.TryParse(HistoryLength, out int historyLengthAsInt))
            {
                historyLengthAsInt = Numbers.HistoryLengthDefault;
                HistoryLength = historyLengthAsInt.ToString();
            }
            if (_currentHistoryLength == historyLengthAsInt)
                return;
            Settings.Instance.SetResultsHistoryLength(historyLengthAsInt);
            // Clear storage for out of bounds results
            var newHistoryLengthIsZero = historyLengthAsInt == 0;
            if (historyLengthAsInt - _currentHistoryLength < 0
                && Settings.Instance.ContainsResultsHistory()
                && !newHistoryLengthIsZero)
            {
                var resultsHistory = await Settings.Instance.GetResultsHistoryAsync();
                if (historyLengthAsInt < resultsHistory.Count)
                {
                    resultsHistory.Reverse();
                    resultsHistory = resultsHistory.Take(historyLengthAsInt).ToList();
                    resultsHistory.Reverse();
                    Settings.Instance.SetResultsHistoryAsync(resultsHistory);
                }
            }
            else if (newHistoryLengthIsZero)
                Settings.Instance.ClearResultsHistory();
            _currentHistoryLength = historyLengthAsInt;
        }

        private void ManageThemeSettings()
        {
            var selectedTheme = _themesDictionary[SelectedTheme];
            if (_currentTheme == selectedTheme)
                return;
            if (selectedTheme.HasValue)
                Settings.Instance.SetTheme(selectedTheme.Value);
            else
                Settings.Instance.ClearTheme();
            Logic.Theming.Instance.ManageAppTheme();
            _currentTheme = selectedTheme;
        }

        private bool CanExecuteSaveSettings() =>
            SettingsChanged;
    }
}