﻿using Calculator.Mobile.DependencyServices;
using Calculator.Mobile.Themes;
using Calculator.Shared.Models.Theming;
using Calculator.Shared.PlatformServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Calculator.Mobile.PlatformServices
{
    class ThemingService : IThemingService
    {
        private static readonly IThemingDependencyService _themingDependencyService
            = DependencyService.Get<IThemingDependencyService>();

        public bool DeviceSupportsManualDarkMode() =>
            _themingDependencyService.DeviceSupportsManualDarkMode();

        public bool DeviceSupportsAutomaticDarkMode() =>
            _themingDependencyService.DeviceSupportsAutomaticDarkMode();

        public Theme GetDeviceDefaultTheme() =>
            _themingDependencyService.GetDeviceDefaultTheme();

        public async Task<Theme> GetDeviceTheme() =>
            await _themingDependencyService.GetDeviceTheme();

        public void SetTheme(Theme theme)
        {
            ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            if (mergedDictionaries != null)
            {
                mergedDictionaries.Clear();
                switch (theme)
                {
                    case Theme.Dark:
                        mergedDictionaries.Add(new DarkTheme());
                        break;
                    case Theme.Light:
                        mergedDictionaries.Add(new LightTheme());
                        break;
                }
            }

            if (_themingDependencyService.DeviceRequiresPagesRedraw())
                RedrawPages();
        }

        private void RedrawPages()
        {
            var firstPage = Application.Current.MainPage.Navigation.NavigationStack[0];
            var originalCount = Application.Current.MainPage.Navigation.NavigationStack.Count;
            for (int i = 0; i < originalCount; i++)
            {
                var page = Application.Current.MainPage.Navigation.NavigationStack[i * 2];
                Application.Current.MainPage.Navigation.InsertPageBefore((Page)Activator.CreateInstance(page.GetType()), firstPage);
            }
            var newCount = Application.Current.MainPage.Navigation.NavigationStack.Count;
            if (newCount == originalCount * 2)
            {
                var pageToPopIndex = originalCount;
                while (Application.Current.MainPage.Navigation.NavigationStack.Count - 1 > pageToPopIndex)
                    Application.Current.MainPage.Navigation.RemovePage(
                        Application.Current.MainPage.Navigation.NavigationStack[pageToPopIndex]);
                Application.Current.MainPage.Navigation.PopAsync(false);
            }
        }
    }
}
