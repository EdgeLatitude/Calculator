using Android.Views;
using Calculator.Mobile.DependencyServices;
using Plugin.CurrentActivity;
using Xamarin.Forms;

[assembly: Dependency(typeof(Calculator.Mobile.Droid.DependencyServices.ClickSoundDependencyService))]
namespace Calculator.Mobile.Droid.DependencyServices
{
    public class ClickSoundDependencyService : IClickSoundDependencyService
    {
        private Android.Views.View _root;

        public void PlaySound()
        {
            if (_root == null)
                _root = CrossCurrentActivity.Current.Activity.FindViewById(Android.Resource.Id.Content);
            _root.PlaySoundEffect(SoundEffects.Click);
        }
    }
}
