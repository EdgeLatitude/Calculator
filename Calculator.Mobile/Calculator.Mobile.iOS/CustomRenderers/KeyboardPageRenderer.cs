using Calculator.Mobile.Controls;
using Calculator.Shared.Constants;
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
        private const string _keySelector = "KeyCommand:";
        private const string _enterKey = "\r";
        private const string _backspaceKey = "\u0008";
        private const string _copyCharacter = "c";
        private const string _exponentCharacter = "e";
        private const string _rootCharacter = "r";

        private static readonly string[] _parenthesesDecimalSeparatorsAndOperators = new string[]
        {
            LexicalSymbolsAsString.OpeningParenthesis,
            LexicalSymbolsAsString.ClosingParenthesis,
            LexicalSymbolsAsString.Comma,
            LexicalSymbolsAsString.Dot,
            LexicalSymbolsAsString.AdditionOperator,
            LexicalSymbolsAsString.SubstractionOperator,
            LexicalSymbolsAsString.MultiplicationOperator,
            LexicalSymbolsAsString.DivisionOperator,
            LexicalSymbolsAsString.PotentiationOperator,
            LexicalSymbolsAsString.SquareRootOperator,
            LexicalSymbolsAsString.SimpleMultiplicationOperator,
            LexicalSymbolsAsString.SimpleDivisionOperator
        };

        private readonly IList<UIKeyCommand> _keyCommands = new List<UIKeyCommand>();

        private KeyboardPage Page => Element as KeyboardPage;

        public override bool CanBecomeFirstResponder => true;

        protected override void OnElementChanged(VisualElementChangedEventArgs args)
        {
            base.OnElementChanged(args);

            if (_keyCommands.Count == 0)
            {
                var selector = new ObjCRuntime.Selector(_keySelector);

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

                // Add support for parentheses, decimal separators and operators
                foreach (var symbol in _parenthesesDecimalSeparatorsAndOperators)
                {
                    _keyCommands.Add(UIKeyCommand.Create((NSString)symbol, 0, selector));
                    _keyCommands.Add(UIKeyCommand.Create((NSString)symbol, UIKeyModifierFlags.NumericPad, selector));
                }

                // Add support for enter and equals key
                _keyCommands.Add(UIKeyCommand.Create((NSString)_enterKey, 0, selector));
                _keyCommands.Add(UIKeyCommand.Create((NSString)LexicalSymbolsAsString.ResultOperator, 0, selector));
                _keyCommands.Add(UIKeyCommand.Create((NSString)LexicalSymbolsAsString.ResultOperator, UIKeyModifierFlags.NumericPad, selector));

                // Add support for backspace key
                _keyCommands.Add(UIKeyCommand.Create((NSString)_backspaceKey, 0, selector));

                // Add support for special commands (viewable on iPad (>= iOS 9) when holding down ⌘)
                _keyCommands.Add(UIKeyCommand.Create(new NSString(_copyCharacter), UIKeyModifierFlags.Command, selector, new NSString(LocalizedStrings.Copy)));
                _keyCommands.Add(UIKeyCommand.Create(new NSString(_exponentCharacter), UIKeyModifierFlags.Command, selector, new NSString(LocalizedStrings.PotentiationOperator)));
                _keyCommands.Add(UIKeyCommand.Create(new NSString(_rootCharacter), UIKeyModifierFlags.Command, selector, new NSString(LocalizedStrings.SquareRootOperator)));

                foreach (var kc in _keyCommands)
                    AddKeyCommand(kc);
            }
        }

        [Export(_keySelector)]
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
                        case _copyCharacter:
                            Page?.OnKeyCommand(Controls.KeyCommand.Copy);
                            break;
                        case _exponentCharacter:
                            Page?.OnKeyCommand(Controls.KeyCommand.ExponentOperator);
                            break;
                        case _rootCharacter:
                            Page?.OnKeyCommand(Controls.KeyCommand.RootOperator);
                            break;
                    }
                else if (keyCommand.Input == _enterKey
                    || keyCommand.Input == LexicalSymbolsAsString.ResultOperator)
                    Page?.OnKeyCommand(Controls.KeyCommand.Calculate);
                else if (keyCommand.Input == _backspaceKey)
                    Page?.OnKeyCommand(Controls.KeyCommand.Delete);
                else
                    Page?.OnKeyUp(keyCommand.Input);
            }
        }
    }
}
