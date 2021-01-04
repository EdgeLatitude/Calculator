using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Calculator.Mobile.Controls
{
    class HapticFeedbackButton : Button
    {
        public HapticFeedbackButton() =>
            Clicked += HapticFeedbackButton_Clicked;

        private void HapticFeedbackButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                HapticFeedback.Perform(HapticFeedbackType.Click);
            }
            catch (Exception) { }
        }
    }
}
