using Calculator.Mobile.DependencyServices;
using Calculator.Shared.Theming;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(Calculator.Mobile.iOS.DependencyServices.Environment))]
namespace Calculator.Mobile.iOS.DependencyServices
{
    public class Environment : IEnvironment
    {
        public bool DeviceSupportsManualDarkMode() =>
            false;

        public bool DeviceSupportsAutomaticDarkMode() =>
            // Ensure the current device is running 12.0 or higher, 
            // because TraitCollection.UserInterfaceStyle was introduced in iOS 12.0
            UIDevice.CurrentDevice.CheckSystemVersion(12, 0);

        public Theme GetDeviceDefaultTheme() =>
            Theme.Light;

        public async Task<Theme> GetDeviceTheme()
        {
            if (DeviceSupportsAutomaticDarkMode())
            {
                var currentUIViewController = await GetVisibleViewController();
                var userInterfaceStyle = currentUIViewController.TraitCollection.UserInterfaceStyle;
                switch (userInterfaceStyle)
                {
                    case UIUserInterfaceStyle.Dark:
                        return Theme.Dark;
                    case UIUserInterfaceStyle.Light:
                        return Theme.Light;
                }
            }

            return GetDeviceDefaultTheme();
        }

        private static Task<UIViewController> GetVisibleViewController() =>
            // UIApplication.SharedApplication can only be referenced on by Main Thread,
            // so it should be used Device.InvokeOnMainThreadAsync which was introduced in Xamarin.Forms v4.2.0
            Device.InvokeOnMainThreadAsync(() =>
            {
                var rootController = UIApplication.SharedApplication.KeyWindow.RootViewController;
                switch (rootController.PresentedViewController)
                {
                    case UINavigationController navigationController:
                        return navigationController.TopViewController;
                    case UITabBarController tabBarController:
                        return tabBarController.SelectedViewController;
                    case null:
                        return rootController;
                    default:
                        return rootController.PresentedViewController;
                }
            });
    }
}
