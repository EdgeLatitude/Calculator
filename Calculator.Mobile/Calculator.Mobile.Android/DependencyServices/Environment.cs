using Android.Content.Res;
using Calculator.Mobile.DependencyServices;
using Calculator.Shared.Theming;
using Plugin.CurrentActivity;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(Calculator.Mobile.Droid.DependencyServices.Environment))]
namespace Calculator.Mobile.Droid.DependencyServices
{
    public class Environment : IEnvironment
    {
        public bool DeviceSupportsManualDarkMode() =>
            true;

        public bool DeviceSupportsAutomaticDarkMode() =>
            true;

        public Theme GetDeviceDefaultTheme() =>
            Theme.Dark;

        public Task<Theme> GetDeviceTheme()
        {
            if (DeviceSupportsAutomaticDarkMode())
            {
                var uiModeFlags = CrossCurrentActivity.Current.AppContext.Resources.Configuration.UiMode
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
