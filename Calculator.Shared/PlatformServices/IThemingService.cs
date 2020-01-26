using Calculator.Shared.Models.Theming;
using System.Threading.Tasks;

namespace Calculator.Shared.PlatformServices
{
    public interface IThemingService
    {
        bool DeviceSupportsManualDarkMode();
        bool DeviceSupportsAutomaticDarkMode();
        Theme GetDeviceDefaultTheme();
        Task<Theme> GetDeviceTheme();
        void SetTheme(Theme theme);
    }
}
