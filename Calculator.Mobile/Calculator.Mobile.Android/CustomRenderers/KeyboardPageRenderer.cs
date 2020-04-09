using Android.Content;
using Android.Runtime;
using Android.Views;
using Calculator.Mobile.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(KeyboardPage), typeof(Calculator.Mobile.Droid.CustomRenderers.KeyboardPageRenderer))]
namespace Calculator.Mobile.Droid.CustomRenderers
{
    [Preserve(AllMembers = true)]
    public class KeyboardPageRenderer : PageRenderer
    {
        private KeyboardPage Page => Element as KeyboardPage;

        public KeyboardPageRenderer(Context context) : base(context)
        {
            Focusable = true;
            FocusableInTouchMode = true;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Page> args)
        {
            base.OnElementChanged(args);

            if (Visibility == ViewStates.Visible)
                RequestFocus();

            Page.Appearing += (sender, innerArgs) =>
            {
                RequestFocus();
            };
        }

        public override bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent keyEvent)
        {
            var handled = false;

            if (keyEvent.IsCtrlPressed)
                switch (keyCode)
                {
                    case Keycode.C:
                        Page?.OnKeyCommand(KeyCommand.Copy);
                        handled = true;
                        break;
                }
            else
            {
                if (keyCode >= Keycode.A
                    && keyCode <= Keycode.Z)
                    handled = true;
                else if ((keyCode >= Keycode.Num0
                            && keyCode <= Keycode.Num9)
                        || (keyCode >= Keycode.Numpad0
                            && keyCode <= Keycode.Numpad9))
                    handled = true;

                if (handled)
                    Page?.OnKeyUp(keyCode.ToString());
            }

            return handled || base.OnKeyUp(keyCode, keyEvent);
        }
    }
}
