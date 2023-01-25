using Xamarin.Forms;

namespace Calculator.Mobile.Controls
{
    public abstract class KeyboardPage : ContentPage
    {
        public abstract void OnKeyUp(char character);
        public abstract void OnKeyCommand(KeyCommand command);
    }

    public enum KeyCommand
    {
        Copy,
        Paste,
        RootOperator,
        Calculate,
        Delete
    }
}
