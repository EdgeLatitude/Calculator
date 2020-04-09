using System.Threading.Tasks;

namespace Calculator.Shared.PlatformServices
{
    public interface IClipboardService
    {
        Task SetTextAsync(string text);
    }
}
