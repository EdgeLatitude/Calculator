using Calculator.Shared.ViewModels;
using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Calculator.Mobile.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalculatorPage : ContentPage
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

        private async void InputLabel_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(Width))
                return;

            await InputScrollView.ScrollToAsync(InputLabel, _viewModel.AfterResult ? ScrollToPosition.Start : ScrollToPosition.End, false);
        }

        private void InputLabel_Tapped(object sender, EventArgs args)
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
