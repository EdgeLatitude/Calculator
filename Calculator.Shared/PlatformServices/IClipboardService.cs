using System.Threading.Tasks;

namespace Calculator.Shared.PlatformServices
{
    public interface IClipboardService
    {
        Task<string> GetTextAsync();
        Task SetTextAsync(string text);
    }
}
