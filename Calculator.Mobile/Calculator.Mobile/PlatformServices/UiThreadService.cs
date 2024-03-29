﻿using Calculator.Shared.PlatformServices;
using System;
using Xamarin.Forms;

namespace Calculator.Mobile.PlatformServices
{
    internal class UiThreadService : IUiThreadService
    {
        public void ExecuteOnUiThread(Action action) =>
            Device.BeginInvokeOnMainThread(action);
    }
}
