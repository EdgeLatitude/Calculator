using Calculator.Mobile.DependencyServices;
using Calculator.Mobile.Themes;
using Calculator.Shared.Models.Theming;
using Calculator.Shared.PlatformServices;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Calculator.Mobile.PlatformServices
{
    internal class ThemingService : IThemingService
    {
        private readonly IThemingDependencyService _themingDependencyService
            = DependencyService.Get<IThemingDependencyService>();

        public bool DeviceSupportsManualDarkMode() =>
            _themingDependencyService.DeviceSupportsManualDarkMode();

        public bool DeviceSupportsAutomaticDarkMode() =>
            _themingDependencyService.DeviceSupportsAutomaticDarkMode();

        public Theme GetDeviceDefaultTheme() =>
            _themingDependencyService.GetDeviceDefaultTheme();

        public Task<Theme> GetDeviceThemeAsync() =>
            _themingDependencyService.GetDeviceThemeAsync();

        public async Task SetThemeAsync(Theme theme)
        {
            var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            var appJustLaunched = true;
            if (mergedDictionaries != null)
            {
                if (mergedDictionaries.Any())
                    appJustLaunched = false;
                if (!appJustLaunched)
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

            if (!appJustLaunched
                && _themingDependencyService.DeviceRequiresPagesRedraw())
                await RedrawPagesAsync();
        }

        private async Task RedrawPagesAsync()
        {
            var firstPage = Application.Current.MainPage.Navigation.NavigationStack[0];
            var originalCount = Application.Current.MainPage.Navigation.NavigationStack.Count;
            for (var i = 0; i < originalCount; i++)
            {
                var page = Application.Current.MainPage.Navigation.NavigationStack[i * 2];
                Application.Current.MainPage.Navigation.InsertPageBefore(await ViewModelLocator.Instance.ResolvePageAsync(page.GetType()), firstPage);
            }
            var newCount = Application.Current.MainPage.Navigation.NavigationStack.Count;
            if (newCount == originalCount * 2)
            {
                var pageToPopIndex = originalCount;
                while (Application.Current.MainPage.Navigation.NavigationStack.Count - 1 > pageToPopIndex)
                    Application.Current.MainPage.Navigation.RemovePage(
                        Application.Current.MainPage.Navigation.NavigationStack[pageToPopIndex]);
                await Application.Current.MainPage.Navigation.PopAsync(false);
            }
        }
    }
}
