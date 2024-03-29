﻿using Calculator.Mobile.DependencyServices;
using System;
using System.Diagnostics;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Calculator.Mobile.Controls
{
    internal class HapticButton : Button
    {
        private static IClickSoundDependencyService _clickSoundService;

        public HapticButton() =>
            Clicked += HapticFeedbackButton_Clicked;

        ~HapticButton() =>
            Clicked -= HapticFeedbackButton_Clicked;

        private void HapticFeedbackButton_Clicked(object sender, EventArgs args)
        {
            try
            {
                _clickSoundService ??= DependencyService.Get<IClickSoundDependencyService>();
                _clickSoundService.PlaySound();
                HapticFeedback.Perform(HapticFeedbackType.Click);
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"{nameof(HapticFeedbackButton_Clicked)} exception: {exception.Message}");
            }
        }
    }
}
