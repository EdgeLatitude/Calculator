using Calculator.Shared.PlatformServices;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Calculator.Mobile.PlatformServices
{
    class NavigationService : INavigationService
    {
        public async Task NavigateToAsync(string resource)
        {
            if (Uri.TryCreate(resource, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp
                    || uri.Scheme == Uri.UriSchemeHttps))
                await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            else
                await Shell.Current.GoToAsync(resource);
        }

        public async Task NavigateBackAsync() =>
            await Shell.Current.Navigation.PopAsync();

        public async Task NavigateBackToRootAsync() =>
            await Shell.Current.Navigation.PopToRootAsync();
    }
}
