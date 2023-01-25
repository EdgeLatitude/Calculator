using Calculator.Shared.PlatformServices;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Calculator.Mobile.PlatformServices
{
    internal class ClipboardService : IClipboardService
    {
        public Task<string> GetTextAsync() =>
            Clipboard.GetTextAsync();

        public Task SetTextAsync(string text) =>
            Clipboard.SetTextAsync(text);
    }
}
