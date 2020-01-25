using Calculator.Mobile.Pages;
using Calculator.Shared.Constants;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Calculator.Mobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell : Shell
    {
        private readonly Dictionary<string, Type> _locationPageDictionary = new Dictionary<string, Type>
        {
            { Locations.SettingsPage, typeof(SettingsPage) }
        };

        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            foreach (var locationPagePair in _locationPageDictionary)
                Routing.RegisterRoute(locationPagePair.Key, locationPagePair.Value);
        }
    }
}
