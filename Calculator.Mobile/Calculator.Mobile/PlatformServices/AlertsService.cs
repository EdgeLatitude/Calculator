using Calculator.Shared.Localization;
using Calculator.Shared.PlatformServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Calculator.Mobile.PlatformServices
{
    internal class AlertsService : IAlertsService
    {
        public Task DisplayAlertAsync(string title, string message) =>
            Application.Current.MainPage.DisplayAlert(title, message, LocalizedStrings.Ok);

        public Task<bool> DisplayConfirmationAsync(string title, string message, string action) =>
            Application.Current.MainPage.DisplayAlert(title, message, action, LocalizedStrings.Cancel);

        public Task<string> DisplayOptionsAsync(string title, string destruction, params string[] options) =>
            Application.Current.MainPage.DisplayActionSheet(title, LocalizedStrings.Cancel, destruction, options);
    }
}
