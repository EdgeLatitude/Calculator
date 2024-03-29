﻿using Android.Content.Res;
using Calculator.Mobile.DependencyServices;
using Calculator.Shared.Models.Theming;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(Calculator.Mobile.Droid.DependencyServices.ThemingDependencyService))]
namespace Calculator.Mobile.Droid.DependencyServices
{
    public class ThemingDependencyService : IThemingDependencyService
    {
        public bool DeviceSupportsManualDarkMode() =>
            true;

        public bool DeviceSupportsAutomaticDarkMode() =>
            true;

        public bool DeviceRequiresPagesRedraw() =>
            true;

        public Theme GetDeviceDefaultTheme() =>
            Theme.Dark;

        public Task<Theme> GetDeviceThemeAsync()
        {
            if (DeviceSupportsAutomaticDarkMode())
            {
                var uiModeFlags = Platform.AppContext.Resources.Configuration.UiMode
                    & UiMode.NightMask;
                switch (uiModeFlags)
                {
                    case UiMode.NightYes:
                        return Task.FromResult(Theme.Dark);
                    case UiMode.NightNo:
                        return Task.FromResult(Theme.Light);
                }
            }
            return Task.FromResult(GetDeviceDefaultTheme());
        }
    }
}
