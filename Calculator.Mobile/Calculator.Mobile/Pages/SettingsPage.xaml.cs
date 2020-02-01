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
        private bool _themePickerIsShowing;

        public SettingsPage()
        {
            InitializeComponent();
            BindingContext = ViewModelLocator.Instance.Resolve<SettingsViewModel>();
        }

        private void ThemePicker_Focused(object sender, FocusEventArgs args)
        {
            if (Device.RuntimePlatform != Device.Android)
                return;

            var themePicker = (Picker)sender;
            themePicker.Unfocus();

            Device.BeginInvokeOnMainThread(async () =>
            {
                if (_themePickerIsShowing)
                    return;

                _themePickerIsShowing = true;

                var senderPickerItems = themePicker.Items.ToArray();

                var selectedItem = await Application.Current.MainPage.DisplayActionSheet(
                    null,
                    LocalizedStrings.Cancel,
                    null,
                    senderPickerItems);

                if (senderPickerItems.Contains(selectedItem))
                    themePicker.SelectedItem = selectedItem;

                _themePickerIsShowing = false;
            });
        }
    }
}
