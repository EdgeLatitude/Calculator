using Android.App;
using Android.Content.PM;
using Android.OS;
using Calculator.Shared.Models.Theming;

namespace Calculator.Mobile.Droid
{
    [Activity(
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
        Theme = "@style/LaunchTheme")]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(true);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            global::Xamarin.Forms.FormsMaterial.Init(this, savedInstanceState);

            LoadApplication(new App());

            Shared.Logic.Theming.ThemeChangeNeeded += GlobalEvents_ThemeChangeNeeded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Shared.Logic.Theming.ThemeChangeNeeded -= GlobalEvents_ThemeChangeNeeded;
        }

        private void GlobalEvents_ThemeChangeNeeded(object sender, ThemeChangeNeededEventArgs args)
        {
            switch (args.Theme)
            {
                case Shared.Models.Theming.Theme.Dark:
                    SetTheme(Resource.Style.MainTheme);
                    break;
                case Shared.Models.Theming.Theme.Light:
                    SetTheme(Resource.Style.MainTheme_Light);
                    break;
            }
        }
    }
}
