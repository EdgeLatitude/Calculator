using Calculator.Shared.PlatformServices;
using Calculator.Shared.Theming;
using System.Threading.Tasks;

namespace Calculator.Shared.Logic
{
    public class Theming
    {
        private static IThemingService _themingService;

        public static Theming Instance
        {
            get;
            private set;
        }

        public static void Initialize(
            IThemingService themingService) =>
            Instance = new Theming(themingService);

        private Theming(
            IThemingService themingService)
        {
            _themingService = themingService;

            DeviceSupportsManualDarkMode = _themingService.DeviceSupportsManualDarkMode();
            DeviceSupportsAutomaticDarkMode = _themingService.DeviceSupportsAutomaticDarkMode();
        }

        public readonly bool DeviceSupportsManualDarkMode;

        public readonly bool DeviceSupportsAutomaticDarkMode;

        private Theme? _currentTheme;

        public async void ManageAppTheme(bool starting = false)
        {
            var theme = await GetAppOrDeviceTheme();
            if (!starting
                && theme == _currentTheme)
                return;
            _themingService.SetTheme(theme);
            _currentTheme = theme;
        }

        public Theme? GetAppOrDefaultTheme() =>
            Settings.Instance.ContainsTheme() ?
                Settings.Instance.GetTheme() :
                DeviceSupportsAutomaticDarkMode ?
                    (Theme?)null :
                    _themingService.GetDeviceDefaultTheme();

        public async Task<Theme> GetAppOrDeviceTheme() =>
            Settings.Instance.ContainsTheme() ?
                Settings.Instance.GetTheme() :
                await _themingService.GetDeviceTheme();
    }
}
