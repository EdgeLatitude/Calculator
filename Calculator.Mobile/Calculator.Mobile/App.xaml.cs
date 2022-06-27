using Calculator.Shared.Constants;
using Calculator.Shared.Logic;
using Xamarin.Forms;

namespace Calculator.Mobile
{
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

        protected override void OnStart()
        {
            base.OnStart();
            _theming.ManageAppTheme(true);
        }

        protected override void OnResume()
        {
            base.OnResume();
            _theming.ManageAppTheme();
        }
    }
}
