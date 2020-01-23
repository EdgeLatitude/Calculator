using System.Threading.Tasks;

namespace Calculator.Shared.PlatformServices
{
    interface ILocalStorageService
    {
        Task<T> Get<T>(string key, T defaultValue);
        Task Set<T>(string key, T value);
        Task<bool> Contains(string key);
        Task Remove(string key);
        Task Clear();
    }
}
