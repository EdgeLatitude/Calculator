using Calculator.Shared.Localization;
using Calculator.Shared.PlatformServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Calculator.Mobile.PlatformServices
{
    class AlertsService : IAlertsService
    {
        public async Task DisplayAlertAsync(string title, string message) =>
            await Shell.Current.DisplayAlert(title, message, LocalizedStrings.Ok);

        public async Task<bool> DisplayConfirmationAsync(string title, string message, string action) =>
            await Shell.Current.DisplayAlert(title, message, action, LocalizedStrings.Cancel);

        public async Task<string> DisplayOptionsAsync(string title, string destruction, params string[] options) =>
            await Shell.Current.DisplayActionSheet(title, LocalizedStrings.Cancel, destruction, options);
    }
}
