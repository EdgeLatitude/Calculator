using Calculator.Shared.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Calculator.Mobile.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalculatorPage : ContentPage
    {
        private readonly CalculatorViewModel _viewModel;

        public CalculatorPage()
        {
            InitializeComponent();
            _viewModel = ViewModelLocator.Instance.Resolve<CalculatorViewModel>();
            BindingContext = _viewModel;
        }

        private async void InputLabel_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(Width))
                return;

            await InputScrollView.ScrollToAsync(InputLabel, _viewModel.AfterResult ? ScrollToPosition.Start : ScrollToPosition.End, false);
        }
    }
}
