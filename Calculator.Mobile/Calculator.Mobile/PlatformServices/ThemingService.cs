using Calculator.Mobile.DependencyServices;
using Calculator.Mobile.Themes;
using Calculator.Shared.Models.Theming;
using Calculator.Shared.PlatformServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Calculator.Mobile.PlatformServices
{
    class ThemingService : IThemingService
    {
        private static readonly IThemingDependencyService themingDependencyService
            = DependencyService.Get<IThemingDependencyService>();

        public bool DeviceSupportsManualDarkMode() =>
            themingDependencyService.DeviceSupportsManualDarkMode();

        public bool DeviceSupportsAutomaticDarkMode() =>
            themingDependencyService.DeviceSupportsAutomaticDarkMode();

        public Theme GetDeviceDefaultTheme() =>
            themingDependencyService.GetDeviceDefaultTheme();

        public async Task<Theme> GetDeviceTheme() =>
            await themingDependencyService.GetDeviceTheme();

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
        }
    }
}
