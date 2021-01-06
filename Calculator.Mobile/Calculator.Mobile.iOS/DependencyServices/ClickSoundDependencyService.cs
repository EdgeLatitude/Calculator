using Calculator.Mobile.DependencyServices;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(Calculator.Mobile.iOS.DependencyServices.ClickSoundDependencyService))]
namespace Calculator.Mobile.iOS.DependencyServices
{
    public class ClickSoundDependencyService : IClickSoundDependencyService
    {
        public void PlaySound() =>
            UIDevice.CurrentDevice.PlayInputClick();
    }
}
