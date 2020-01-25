using Autofac;
using Calculator.Mobile.PlatformServices;
using Calculator.Shared.PlatformServices;
using Calculator.Shared.ViewModels;

namespace Calculator.Mobile
{
    static class ViewModelLocator
    {
        private static readonly IContainer _container;

        static ViewModelLocator()
        {
            var builder = new ContainerBuilder();
            RegisterPlatformServices(builder);
            RegisterViewModels(builder);
            _container = builder.Build();
            InitializeSingletons();
        }

        private static void RegisterPlatformServices(ContainerBuilder builder)
        {
            builder.RegisterType<AlertsService>().As<IAlertsService>().SingleInstance();
            builder.RegisterType<CommandFactoryService>().As<ICommandFactoryService>().SingleInstance();
            builder.RegisterType<NavigationService>().As<INavigationService>().SingleInstance();
            builder.RegisterType<SettingsService>().As<ISettingsService>().SingleInstance();
            builder.RegisterType<ThemingService>().As<IThemingService>().SingleInstance();
            builder.RegisterType<UiThreadService>().As<IUiThreadService>().SingleInstance();
        }

        private static void RegisterViewModels(ContainerBuilder builder)
        {
            builder.RegisterType<CalculatorViewModel>().SingleInstance();
            builder.RegisterType<SettingsViewModel>().SingleInstance();
        }

        private static void InitializeSingletons()
        {
            Shared.Logic.Settings.Initialize(
                _container.Resolve<ISettingsService>(),
                _container.Resolve<IThemingService>());
            Shared.Logic.Theming.Initialize(
                _container.Resolve<IThemingService>());
        }

        public static TViewModel Resolve<TViewModel>() where TViewModel : BaseViewModel =>
            _container.Resolve<TViewModel>();
    }
}
