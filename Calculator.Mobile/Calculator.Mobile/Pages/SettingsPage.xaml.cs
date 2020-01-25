using Calculator.Shared.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Calculator.Mobile.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            BindingContext = ViewModelLocator.Resolve<SettingsViewModel>();
        }
    }
}
