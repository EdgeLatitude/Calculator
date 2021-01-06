using Calculator.Mobile.DependencyServices;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Calculator.Mobile.Controls
{
    class HapticFeedbackButton : Button
    {
        private static IClickSoundDependencyService _clickSoundService;

        public HapticFeedbackButton() =>
            Clicked += HapticFeedbackButton_Clicked;

        private void HapticFeedbackButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (_clickSoundService == null)
                    _clickSoundService = DependencyService.Get<IClickSoundDependencyService>();
                _clickSoundService.PlaySound();
                HapticFeedback.Perform(HapticFeedbackType.Click);
            }
            catch (Exception) { }
        }
    }
}
