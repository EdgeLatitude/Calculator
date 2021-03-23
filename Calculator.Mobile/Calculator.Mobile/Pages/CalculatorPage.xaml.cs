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
        private readonly CalculatorViewModel _viewModel;

        private bool _inputCopiedToClipboardToastIsVisible;
        private int _inputCopiedToClipboardToastActiveTaps;

        public CalculatorPage()
        {
            InitializeComponent();
            _viewModel = ViewModelLocator.Instance.Resolve<CalculatorViewModel>();
            BindingContext = _viewModel;
        }

        public override void OnKeyUp(char character) =>
            _viewModel.ManageInputFromHardwareCommand.Execute(char.ToString(character));

        public override void OnKeyCommand(KeyCommand command)
        {
            switch (command)
            {
                case KeyCommand.Copy:
                    _viewModel.CopyInputToClipboardCommand.Execute(null);
                    CopyInputToClipboardAnimation();
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

        private async void InputStackLayout_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(Width))
                return;

            await InputScrollView.ScrollToAsync(InputStackLayout, _viewModel.AfterResult ?
                ScrollToPosition.Start :
                ScrollToPosition.End,
                false);
        }

        private void CopyInputToClipboard()
        {
            if (!_viewModel.Input.Any())
                return;
            _viewModel.CopyInputToClipboardCommand.Execute(null);
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
