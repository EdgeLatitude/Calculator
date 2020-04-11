using Xamarin.Forms;

namespace Calculator.Mobile.Controls
{
    public class KeyboardPage : ContentPage
    {
        public virtual void OnKeyUp(string character) { return; }
        public virtual void OnKeyCommand(KeyCommand command) { return; }
    }

    public enum KeyCommand
    {
        Copy,
        SquareRootOperator,
        Calculate
    }
}
