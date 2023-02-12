using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.AppCompat.App;
using System.Threading.Tasks;

namespace Calculator.Mobile.Droid
{
    [Activity(
        Icon = "@mipmap/icon",
        Label = "@string/app_name",
        MainLauncher = true,
        NoHistory = true,
        Theme = "@style/MainTheme.Splash")]
    public class LaunchScreenActivity : AppCompatActivity
    {
        private bool _mainActivityCreationStarted;

        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (_mainActivityCreationStarted)
                return;
            new Task(() =>
            {
                using var mainActivityIntent = new Intent(Application.Context, typeof(MainActivity));
                mainActivityIntent.AddFlags(ActivityFlags.NoAnimation);
                StartActivity(mainActivityIntent);
            }).Start();
            _mainActivityCreationStarted = true;
        }

        public override void OnBackPressed() { }
    }
}
