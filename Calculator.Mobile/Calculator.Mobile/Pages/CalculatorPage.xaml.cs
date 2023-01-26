using Calculator.Mobile.Controls;
using Calculator.Shared.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Calculator.Mobile.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalculatorPage : KeyboardPage
    {
        private const double _minimumLayoutHeight = 240d;

        private readonly CalculatorViewModel _viewModel;

        private bool _inputCopiedToClipboardToastIsVisible;
        private int _inputCopiedToClipboardToastActiveTaps;

        public CalculatorPage()
        {
            InitializeComponent();
            _viewModel = ViewModelLocator.Instance.ResolveViewModel<CalculatorViewModel>();
            BindingContext = _viewModel;
            InputStackLayout.PropertyChanged += InputStackLayout_PropertyChanged;
        }

        ~CalculatorPage()
        {
            InputStackLayout.PropertyChanged -= InputStackLayout_PropertyChanged;
        }

        public override void OnKeyUp(char character) =>
            _viewModel.ManageInputCharacterCommand.Execute(char.ToString(character));

        public override void OnKeyCommand(KeyCommand command)
        {
            switch (command)
            {
                case KeyCommand.Copy:
                    _viewModel.CopyCommand.Execute(null);
                    CopyInputToClipboardAnimation();
                    break;
                case KeyCommand.Paste:
                    _viewModel.PasteCommand.Execute(null);
                    break;
                case KeyCommand.RootOperator:
                    SquareRootButton.Command.Execute(SquareRootButton.CommandParameter);
                    break;
                case KeyCommand.Calculate:
                    CalculateButton.Command.Execute(null);
                    break;
                case KeyCommand.Delete:
                    DeleteButton.Command.Execute(null);
                    break;
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (KeypadScrollView.Width > KeypadScrollView.Height
                && KeypadScrollView.Height < _minimumLayoutHeight)
            {
                StandardKeypadLayout.IsVisible = false;
                WideKeypadLayout.IsVisible = true;
            }
            else
            {
                WideKeypadLayout.IsVisible = false;
                StandardKeypadLayout.IsVisible = true;
            }
        }

        private async void InputStackLayout_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(Width))
                return;

            await InputScrollView.ScrollToAsync(InputStackLayout, _viewModel.AfterResult ?
                ScrollToPosition.Start :
                ScrollToPosition.End,
                false);
        }

        private void Copy_Tapped(object sender, EventArgs args)
        {
            if (!_viewModel.Input.Any())
                return;
            CopyInputToClipboardAnimation();
        }

        private void CopyInputToClipboardAnimation()
        {
            if (!_inputCopiedToClipboardToastIsVisible)
            {
                InputCopiedToClipboardToast.FadeTo(0.75);
                _inputCopiedToClipboardToastIsVisible = true;
            }

            _inputCopiedToClipboardToastActiveTaps++;

            Device.StartTimer(TimeSpan.FromSeconds(3.75), () =>
            {
                _inputCopiedToClipboardToastActiveTaps--;

                if (_inputCopiedToClipboardToastActiveTaps == 0)
                {
                    InputCopiedToClipboardToast.FadeTo(0);
                    _inputCopiedToClipboardToastIsVisible = false;
                }

                return false;
            });
        }
    }
}
