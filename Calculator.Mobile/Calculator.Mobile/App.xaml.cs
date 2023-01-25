using Calculator.Shared.Constants;
using Calculator.Shared.Logic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Calculator.Mobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class App : Application
    {
        private readonly Theming _theming;

        public App()
        {
            InitializeComponent();
            ViewModelLocator.Initialize();
            MainPage = new NavigationPage(ViewModelLocator.Instance.ResolvePage(Locations.CalculatorPage));
            _theming = ViewModelLocator.Instance.Resolve<Theming>();
        }

        protected override async void OnStart()
        {
            base.OnStart();
            await _theming.ManageAppThemeAsync(true);
        }

        protected override async void OnResume()
        {
            base.OnResume();
            await _theming.ManageAppThemeAsync();
        }
    }
}
