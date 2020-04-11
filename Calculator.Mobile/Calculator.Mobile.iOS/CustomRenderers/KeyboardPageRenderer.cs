using Calculator.Mobile.Controls;
using Calculator.Shared.Localization;
using Foundation;
using System.Collections.Generic;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(KeyboardPage), typeof(Calculator.Mobile.iOS.CustomRenderers.KeyboardPageRenderer))]
namespace Calculator.Mobile.iOS.CustomRenderers
{
    public class KeyboardPageRenderer : PageRenderer
    {
        private const string KeySelector = "KeyCommand:";
        private const string EnterKey = @"\r";
        private const string NumericPadEnterKey = @"\u{3}";
        private const string CopyCharacter = "c";
        private const string SquareRootCharacter = "r";

        private readonly IList<UIKeyCommand> _keyCommands = new List<UIKeyCommand>();

        private KeyboardPage Page => Element as KeyboardPage;

        public override bool CanBecomeFirstResponder => true;

        protected override void OnElementChanged(VisualElementChangedEventArgs args)
        {
            base.OnElementChanged(args);

            if (_keyCommands.Count == 0)
            {
                var selector = new ObjCRuntime.Selector(KeySelector);

                // Add support for numbers
                for (var i = 0; i < 10; i++)
                {
                    _keyCommands.Add(UIKeyCommand.Create((NSString)i.ToString(), 0, selector));
                    _keyCommands.Add(UIKeyCommand.Create((NSString)i.ToString(), UIKeyModifierFlags.NumericPad, selector));
                }

                /* // Add support for alphabet
                for (var i = 0; i < 26; i++)
                {
                    var key = (char)('a' + i);
                    _keyCommands.Add(UIKeyCommand.Create((NSString)key.ToString(), 0, selector));
                }
                */

                // Add support for enter key
                _keyCommands.Add(UIKeyCommand.Create((NSString)EnterKey, 0, selector));
                _keyCommands.Add(UIKeyCommand.Create((NSString)NumericPadEnterKey, 0, selector));

                // Add support for special commands (viewable on iPad (>= iOS 9) when holding down ⌘)
                _keyCommands.Add(UIKeyCommand.Create(new NSString(CopyCharacter), UIKeyModifierFlags.Command, selector, new NSString(LocalizedStrings.Copy)));
                _keyCommands.Add(UIKeyCommand.Create(new NSString(SquareRootCharacter), UIKeyModifierFlags.Command, selector, new NSString(LocalizedStrings.SquareRootOperator)));

                foreach (var kc in _keyCommands)
                    AddKeyCommand(kc);
            }
        }

        [Export(KeySelector)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051: Remove unused member", Justification = "It is used but not detected by the IDE")]
        private void KeyCommand(UIKeyCommand keyCommand)
        {
            if (keyCommand == null)
                return;

            if (_keyCommands.Contains(keyCommand))
            {
                if (keyCommand.ModifierFlags == UIKeyModifierFlags.Command)
                    switch (keyCommand.Input.ToString())
                    {
                        case CopyCharacter:
                            Page?.OnKeyCommand(Controls.KeyCommand.Copy);
                            break;
                        case SquareRootCharacter:
                            Page?.OnKeyCommand(Controls.KeyCommand.SquareRootOperator);
                            break;
                    }
                else if (keyCommand.Input == EnterKey
                    || keyCommand.Input == NumericPadEnterKey)
                    Page?.OnKeyCommand(Controls.KeyCommand.Calculate);
                else
                    Page?.OnKeyUp(keyCommand.Input);
            }
        }
    }
}
