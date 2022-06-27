using Calculator.Shared.Models.Theming;
using System.Threading.Tasks;

namespace Calculator.Shared.PlatformServices
{
    public interface IThemingService
    {
        bool DeviceSupportsManualDarkMode();
        bool DeviceSupportsAutomaticDarkMode();
        Theme GetDeviceDefaultTheme();
        Task<Theme> GetDeviceThemeAsync();
        Task SetThemeAsync(Theme theme);
    }
}
