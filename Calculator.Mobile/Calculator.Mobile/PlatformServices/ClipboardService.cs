using Calculator.Shared.PlatformServices;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Calculator.Mobile.PlatformServices
{
    class ClipboardService : IClipboardService
    {
        public async Task<string> GetTextAsync() =>
            await Clipboard.GetTextAsync();

        public async Task SetTextAsync(string text) =>
            await Clipboard.SetTextAsync(text);
    }
}
