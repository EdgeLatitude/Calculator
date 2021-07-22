﻿using Calculator.Shared.Constants;
using Calculator.Shared.PlatformServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Calculator.Shared.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        #region Fields
        private readonly ICommandFactoryService _commandFactoryService;
        private readonly INavigationService _navigationService;
        private readonly IPlatformInformationService _platformInformationService;
        #endregion

        #region Properties
        private bool _platformSupportsGettingApplicationVersion;

        public bool PlatformSupportsGettingApplicationVersion
        {
            get => _platformSupportsGettingApplicationVersion;
            private set
            {
                if (_platformSupportsGettingApplicationVersion == value)
                    return;
                _platformSupportsGettingApplicationVersion = value;
                OnPropertyChanged();
            }
        }

        private string _appVersion;

        public string AppVersion
        {
            get => _appVersion;
            private set
            {
                if (_appVersion == value)
                    return;
                _appVersion = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        public ICommand NavigateToWebsiteCommand { get; }
        #endregion

        #region Constructors
        public AboutViewModel(
            ICommandFactoryService commandFactoryService,
            INavigationService navigationService,
            IPlatformInformationService platformInformationService)
        {
            _commandFactoryService = commandFactoryService;
            _navigationService = navigationService;
            _platformInformationService = platformInformationService;

            NavigateToWebsiteCommand = _commandFactoryService.Create(async (string website) => await NavigateToWebsite(website));

            PlatformSupportsGettingApplicationVersion = _platformInformationService.PlatformSupportsGettingApplicationVersion();
            if (PlatformSupportsGettingApplicationVersion)
                AppVersion = _platformInformationService.GetApplicationVersion();
        }
        #endregion

        #region Methods
        private async Task NavigateToWebsite(string website) =>
            await _navigationService.NavigateToAsync($"{Strings.HttpsProtocol}{website}");
        #endregion
    }
}
