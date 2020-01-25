using Autofac;
using Calculator.Mobile.PlatformServices;
using Calculator.Shared.PlatformServices;
using Calculator.Shared.ViewModels;
using System;
using System.Collections.Generic;

namespace Calculator.Mobile
{
    static class ViewModelLocator
    {
        private static readonly IContainer _container;

        private static readonly Dictionary<Type, Type> _implementationInterfaceDictionary = new Dictionary<Type, Type>
        {
            { typeof(AlertsService), typeof(IAlertsService) },
            { typeof(CommandFactoryService), typeof(ICommandFactoryService) },
            { typeof(NavigationService), typeof(INavigationService) },
            { typeof(SettingsService), typeof(ISettingsService) },
            { typeof(ThemingService), typeof(IThemingService) },
            { typeof(UiThreadService), typeof(IUiThreadService) }
        };

        private static readonly Type[] _viewModelsToResolve = new Type[]
        {
            typeof(CalculatorViewModel),
            typeof(SettingsViewModel)
        };

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
            foreach (var implementationInterfacePair in _implementationInterfaceDictionary)
                builder.RegisterType(implementationInterfacePair.Key)
                    .As(implementationInterfacePair.Value).SingleInstance();
        }

        private static void RegisterViewModels(ContainerBuilder builder)
        {
            foreach (var viewModelToResolve in _viewModelsToResolve)
                builder.RegisterType(viewModelToResolve).SingleInstance();
        }

        private static void InitializeSingletons()
        {
            Shared.Logic.Settings.Initialize(
                _container.Resolve<ISettingsService>(),
                _container.Resolve<IThemingService>());
            Shared.Logic.Theming.Initialize(
                _container.Resolve<IThemingService>());
        }

        public static TViewModel Resolve<TViewModel>()
            where TViewModel : BaseViewModel =>
            _container.Resolve<TViewModel>();
    }
}
