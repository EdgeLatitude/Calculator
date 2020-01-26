using Calculator.Shared.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Calculator.Mobile.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalculatorPage : ContentPage
    {
        public CalculatorPage()
        {
            InitializeComponent();
            BindingContext = ViewModelLocator.Instance.Resolve<CalculatorViewModel>();
        }

        private void InputEntry_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            var inputEntry = (Entry)sender;
            if (args.PropertyName == Entry.TextProperty.PropertyName)
                inputEntry.CursorPosition = inputEntry.Text.Length;
        }
    }
}
