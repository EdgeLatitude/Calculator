namespace Calculator.Shared.PlatformServices
{
    public interface ISettingsService
    {
        T Get<T>(string key, T defaultValue);
        void Set<T>(string key, T value);
        bool Contains(string key);
        void Remove(string key);
        void Clear();
    }
}
