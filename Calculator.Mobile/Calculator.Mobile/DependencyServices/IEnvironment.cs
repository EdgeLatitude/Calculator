using Calculator.Shared.Theming;
using System.Threading.Tasks;

namespace Calculator.Mobile.DependencyServices
{
    public interface IEnvironment
    {
        bool DeviceSupportsManualDarkMode();
        bool DeviceSupportsAutomaticDarkMode();
        Theme GetDeviceDefaultTheme();
        Task<Theme> GetDeviceTheme(); // Implementation for this based on https://codetraveler.io/2019/09/11/check-for-dark-mode-in-xamarin-forms/
    }
}
