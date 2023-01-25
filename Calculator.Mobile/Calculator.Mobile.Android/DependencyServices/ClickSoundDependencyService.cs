using Android.Views;
using Calculator.Mobile.DependencyServices;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(Calculator.Mobile.Droid.DependencyServices.ClickSoundDependencyService))]
namespace Calculator.Mobile.Droid.DependencyServices
{
    public class ClickSoundDependencyService : IClickSoundDependencyService
    {
        private Android.Views.View _root;

        public void PlaySound()
        {
            _root ??= Platform.CurrentActivity.FindViewById(Android.Resource.Id.Content);
            _root.PlaySoundEffect(SoundEffects.Click);
        }
    }
}
