﻿using Calculator.Mobile.Pages;
using Calculator.Shared.Logic;
using Xamarin.Forms;

namespace Calculator.Mobile
{
    public partial class App : Application
    {
        public App()
        {
            Shared.Logic.Calculator.Initialize();
            ViewModelLocator.Initialize();
            InitializeComponent();
            MainPage = new NavigationPage(new CalculatorPage());
        }

        protected override void OnStart()
        {
            base.OnStart();
            Theming.Instance.ManageAppTheme(true);
        }

        protected override void OnResume()
        {
            base.OnResume();
            Theming.Instance.ManageAppTheme();
        }
    }
}
