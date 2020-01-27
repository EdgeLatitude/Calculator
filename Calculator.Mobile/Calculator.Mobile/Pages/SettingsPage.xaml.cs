using Calculator.Shared.Localization;
using Calculator.Shared.ViewModels;
using System.Linq;
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
            BindingContext = ViewModelLocator.Instance.Resolve<SettingsViewModel>();
        }

        private async void Picker_Focused(object sender, FocusEventArgs e)
        {
            if (Device.RuntimePlatform != Device.Android)
                return;

            var senderPicker = (Picker)sender;
            senderPicker.Unfocus();

            var selectedItem = await Shell.Current.DisplayActionSheet(
                null,
                LocalizedStrings.Cancel,
                null,
                senderPicker.Items.ToArray());

            if (senderPicker.Items.Contains(selectedItem))
                senderPicker.SelectedItem = selectedItem;
        }
    }
}
