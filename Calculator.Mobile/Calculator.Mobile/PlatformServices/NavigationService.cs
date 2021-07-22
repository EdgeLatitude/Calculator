﻿using Calculator.Mobile.Pages;
using Calculator.Shared.Constants;
using Calculator.Shared.PlatformServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Calculator.Mobile.PlatformServices
{
    internal class NavigationService : INavigationService
    {
        private readonly Dictionary<string, Type> _locationPageDictionary = new Dictionary<string, Type>
        {
            { Locations.SettingsPage, typeof(SettingsPage) }
        };

        public async Task NavigateToAsync(string resource)
        {
            if (Uri.TryCreate(resource, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp
                    || uri.Scheme == Uri.UriSchemeHttps))
                await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            else
                await Application.Current.MainPage.Navigation.PushAsync((Page)Activator.CreateInstance(_locationPageDictionary[resource]));
        }

        public async Task NavigateBackAsync() =>
            await Application.Current.MainPage.Navigation.PopAsync();

        public async Task NavigateBackToRootAsync() =>
            await Application.Current.MainPage.Navigation.PopToRootAsync();
    }
}
