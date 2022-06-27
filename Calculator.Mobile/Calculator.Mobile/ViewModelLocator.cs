using Autofac;
using Calculator.Mobile.PlatformServices;
using Calculator.Shared.PlatformServices;
using Calculator.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Calculator.Mobile
{
    internal class ViewModelLocator
    {
        public static ViewModelLocator Instance
        {
            get;
            private set;
        }

        public static void Initialize() =>
            Instance = new ViewModelLocator();

        public TViewModel ResolveViewModel<TViewModel>()
            where TViewModel : BaseViewModel =>
            _container.Resolve<TViewModel>();

        public T Resolve<T>() =>
            _container.Resolve<T>();

        private readonly IContainer _container;

        private readonly IReadOnlyDictionary<Type, Type> _implementationInterfaceDictionary = new ReadOnlyDictionary<Type, Type>(new Dictionary<Type, Type>
        {
            { typeof(AlertsService), typeof(IAlertsService) },
            { typeof(ClipboardService), typeof(IClipboardService) },
            { typeof(CommandFactoryService), typeof(ICommandFactoryService) },
            { typeof(NavigationService), typeof(INavigationService) },
            { typeof(PlatformInformationService), typeof(IPlatformInformationService) },
            { typeof(SettingsService), typeof(ISettingsService) },
            { typeof(ThemingService), typeof(IThemingService) },
            { typeof(UiThreadService), typeof(IUiThreadService) }
        });

        private readonly ReadOnlyCollection<Type> _viewModelsToResolve = new ReadOnlyCollection<Type>(new Type[]
        {
            typeof(AboutViewModel)
        });

        private readonly ReadOnlyCollection<Type> _viewModelsToResolveAsSingletons = new ReadOnlyCollection<Type>(new Type[]
        {
            typeof(CalculatorViewModel),
            typeof(SettingsViewModel)
        });

        private ViewModelLocator()
        {
            var builder = new ContainerBuilder();
            RegisterPlatformServices(builder);
            RegisterViewModels(builder);
            RegisterLogic(builder);
            _container = builder.Build();
        }

        private void RegisterPlatformServices(ContainerBuilder builder)
        {
            foreach (var implementationInterfacePair in _implementationInterfaceDictionary)
                builder.RegisterType(implementationInterfacePair.Key)
                    .As(implementationInterfacePair.Value).SingleInstance();
        }

        private void RegisterViewModels(ContainerBuilder builder)
        {
            foreach (var viewModelToResolve in _viewModelsToResolve)
                builder.RegisterType(viewModelToResolve);
            foreach (var viewModelToResolve in _viewModelsToResolveAsSingletons)
                builder.RegisterType(viewModelToResolve).SingleInstance();
        }

        private void RegisterLogic(ContainerBuilder builder)
        {
            builder.RegisterType<Shared.Logic.Calculator>().InstancePerLifetimeScope();
            builder.RegisterType<Shared.Logic.Settings>().InstancePerLifetimeScope();
            builder.RegisterType<Shared.Logic.Theming>().InstancePerLifetimeScope();
        }
    }
}
