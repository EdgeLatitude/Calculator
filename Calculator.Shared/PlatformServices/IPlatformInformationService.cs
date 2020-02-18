namespace Calculator.Shared.PlatformServices
{
    public interface IPlatformInformationService
    {
        bool PlatformSupportsGettingApplicationVersion();
        string GetApplicationVersion();
    }
}
