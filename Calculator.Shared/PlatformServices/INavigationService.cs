using System.Threading.Tasks;

namespace Calculator.Shared.PlatformServices
{
    interface INavigationService
    {
        Task NavigateToAsync(string resource);
        Task NavigateBackAsync();
        Task NavigateBackToRootAsync();
    }
}
