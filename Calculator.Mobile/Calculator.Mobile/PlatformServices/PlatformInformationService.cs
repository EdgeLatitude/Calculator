using Calculator.Shared.PlatformServices;
using Xamarin.Essentials;

namespace Calculator.Mobile.PlatformServices
{
    class PlatformInformationService : IPlatformInformationService
    {
        public bool PlatformSupportsGettingApplicationVersion() =>
            true;

        public string GetApplicationVersion() =>
            VersionTracking.CurrentVersion;
    }
}
