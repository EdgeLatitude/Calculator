using Calculator.Shared.Constants;
using Calculator.Shared.Localization;
using Calculator.Shared.Logic;
using Calculator.Shared.Models.Enums;
using Calculator.Shared.PlatformServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Calculator.Shared.ViewModels
{
    public class CalculatorViewModel : BaseViewModel
    {
        private readonly IAlertsService _alertsService;
        private readonly IClipboardService _clipboardService;
        private readonly ICommandFactoryService _commandFactoryService;
        private readonly INavigationService _navigationService;
        private readonly IPlatformInformationService _platformInformationService;

        private readonly Dictionary<char, decimal> _variableStorageValues
            = new Dictionary<char, decimal>();

        private bool _calculating;

        private NextInput _nextStroke = NextInput.DoNothing;

        public CalculatorViewModel(
            IAlertsService alertsService,
            IClipboardService clipboardService,
            ICommandFactoryService commandFactoryService,
            INavigationService navigationService,
            IPlatformInformationService platformInformationService)
        {
            _alertsService = alertsService;
            _clipboardService = clipboardService;
            _commandFactoryService = commandFactoryService;
            _navigationService = navigationService;
            _platformInformationService = platformInformationService;

            AllClearCommand = _commandFactoryService.Create(AllClear);
            ClearCommand = _commandFactoryService.Create(Clear);
            DeleteCommand = _commandFactoryService.Create(Delete);
            BinaryOperatorCommand = _commandFactoryService.Create<string>((symbol) => BinaryOperator(symbol));
            UnaryOperatorCommand = _commandFactoryService.Create<string>((symbol) => UnaryOperator(symbol));
            ParenthesesCommand = _commandFactoryService.Create<string>((parentheses) => Parentheses(parentheses));
            LastResultCommand = _commandFactoryService.Create(LastResult);
            NumberCommand = _commandFactoryService.Create<string>((number) => Number(number));
            DecimalCommand = _commandFactoryService.Create(Decimal);
            CalculateCommand = _commandFactoryService.Create(Calculate);
            CopyInputToClipboardCommand = _commandFactoryService.Create(CopyInputToClipboard);
            ShowHistoryCommand = _commandFactoryService.Create(ShowHistory);
            NavigateToSettingsCommand = _commandFactoryService.Create(async () => await NavigateToSettingsAsync());
            ShowAboutCommand = _commandFactoryService.Create(async () => await ShowAbout());
        }

        private string _input = string.Empty;

        public string Input
        {
            get => _input;
            set
            {
                if (_input == value)
                    return;
                _input = value;
                OnPropertyChanged();

                if (!_calculating)
                    AfterResult = false;
            }
        }

        public string DecimalSeparator => Logic.Calculator.DecimalSeparator;

        public bool AfterResult { get; private set; }

        public ICommand AllClearCommand { get; private set; }

        public ICommand ClearCommand { get; private set; }

        public ICommand DeleteCommand { get; private set; }

        public ICommand BinaryOperatorCommand { get; private set; }

        public ICommand UnaryOperatorCommand { get; private set; }

        public ICommand ParenthesesCommand { get; private set; }

        public ICommand LastResultCommand { get; private set; }

        public ICommand NumberCommand { get; private set; }

        public ICommand DecimalCommand { get; private set; }

        public ICommand CalculateCommand { get; private set; }

        public ICommand CopyInputToClipboardCommand { get; private set; }

        public ICommand ShowHistoryCommand { get; private set; }

        public ICommand NavigateToSettingsCommand { get; private set; }

        public ICommand ShowAboutCommand { get; private set; }

        private void AllClear()
        {
            // Clear user input and memory values
            Clear();
            _variableStorageValues.Clear();
        }

        private void Clear()
        {
            // Clear user input
            Input = string.Empty;
            _nextStroke = NextInput.DoNothing;
        }

        private void Delete()
        {
            // Do nothing if there is currently no input
            if (string.IsNullOrWhiteSpace(Input))
                return;
            // Clear everything if required
            if (_nextStroke != NextInput.DoNothing)
            {
                Input = string.Empty;
                _nextStroke = NextInput.DoNothing;
                return;
            }
            // Else only delete 1 character, the last one
            Input = Input[0..^1];
        }

        private void BinaryOperator(string symbol)
        {
            if (_nextStroke == NextInput.ClearAtAny)
            {
                Input = symbol;
                _nextStroke = NextInput.DoNothing;
            }
            else if (_nextStroke == NextInput.ClearAtNumber)
            {
                Input = Logic.Calculator.LastResult + symbol;
                _nextStroke = NextInput.DoNothing;
            }
            else
                Input += symbol;
        }

        private void UnaryOperator(string symbol)
        {
            if (_nextStroke != NextInput.DoNothing)
            {
                Input = symbol;
                _nextStroke = NextInput.DoNothing;
            }
            else
                Input += symbol;
        }

        private void Parentheses(string parentheses)
        {
            if (_nextStroke != NextInput.DoNothing)
            {
                Input = parentheses;
                _nextStroke = NextInput.DoNothing;
            }
            else
                Input += parentheses;
        }

        private void LastResult()
        {
            if (_nextStroke != NextInput.DoNothing)
            {
                Input = char.ToString(Logic.Calculator.LastResult);
                _nextStroke = NextInput.DoNothing;
            }
            else
                Input += Logic.Calculator.LastResult;
        }

        private void Number(string number)
        {
            if (_nextStroke == NextInput.ClearAtAny
                || _nextStroke == NextInput.ClearAtNumber)
            {
                Input = number;
                _nextStroke = NextInput.DoNothing;
            }
            else
                Input += number;
        }

        private void Decimal()
        {
            if (_nextStroke != NextInput.DoNothing)
            {
                Input = Logic.Calculator.DecimalSeparator;
                _nextStroke = NextInput.DoNothing;
            }
            else
                Input += Logic.Calculator.DecimalSeparator;
        }

        private void Calculate()
        {
            // Do nothing if there is no input
            if (string.IsNullOrWhiteSpace(Input))
                return;

            // Clear input if there was no interaction after an error
            if (_nextStroke == NextInput.ClearAtAny)
            {
                Input = string.Empty;
                _nextStroke = NextInput.DoNothing;
                return;
            }

            // Calculate and show corresponding result
            _calculating = true;
            var calculationResult = Logic.Calculator.Calculate(Input, _variableStorageValues);
            if (calculationResult != null)
                // Show result if calculation was successful
                if (calculationResult.Successful)
                {
                    var result = calculationResult.Result;
                    if (TryFormatResult(result, out var resultText))
                    {
                        AfterResult = true;

                        Input = resultText;
                        _nextStroke = NextInput.ClearAtNumber;

                        AddOrUpdateVariableStorage(Logic.Calculator.LastResult, result);

                        _ = Settings.Instance.ManageNewResultAsync(resultText);
                    }
                    // Show error message if result could not be formatted
                    else
                    {
                        Input = LocalizedStrings.CalculationError;
                        _nextStroke = NextInput.ClearAtAny;
                    }

                }
                // Show error message if calculation was not successful
                else
                {
                    Input = calculationResult.ErrorMessage;
                    _nextStroke = NextInput.ClearAtAny;
                }
            // It has no reason to be a null object
            else
            {
                Input = LocalizedStrings.UnexpectedError;
                _nextStroke = NextInput.ClearAtAny;
            }
            _calculating = false;
        }

        private void AddOrUpdateVariableStorage(char storage, decimal value)
        {
            // If memory value already exists, overwrite it, else, add it
            if (_variableStorageValues.TryGetValue(storage, out decimal _))
                _variableStorageValues[storage] = value;
            else
                _variableStorageValues.Add(storage, value);
        }

        private bool TryFormatResult(decimal result, out string resultText)
        {
            resultText = result.ToString();
            while (resultText.Contains(Logic.Calculator.DecimalSeparator)
                && (char.ToString(resultText[^1]) == Logic.Calculator.ZeroString
                    || char.ToString(resultText[^1]) == Logic.Calculator.DecimalSeparator))
                resultText = resultText[0..^1];
            return true;
        }

        private async void CopyInputToClipboard() =>
            await _clipboardService.SetTextAsync(Input);

        private async void ShowHistory()
        {
            if (Settings.Instance.GetResultsHistoryLength() == 0)
            {
                var openSettings = await _alertsService.DisplayConfirmationAsync(LocalizedStrings.Notice,
                    LocalizedStrings.DisabledResultsHistory,
                    LocalizedStrings.Settings);
                if (openSettings)
                    await NavigateToSettingsAsync();
                return;
            }

            if (!Settings.Instance.ContainsResultsHistory())
            {
                await _alertsService.DisplayAlertAsync(LocalizedStrings.Notice,
                    LocalizedStrings.EmptyResultsHistory);
                return;
            }

            var resultsHistory = await Settings.Instance.GetResultsHistoryAsync();
            resultsHistory.Reverse();
            var resultFromHistory = await _alertsService.DisplayOptionsAsync(LocalizedStrings.History,
                LocalizedStrings.ClearHistory,
                resultsHistory.ToArray());
            if (resultFromHistory != null
                && resultFromHistory != LocalizedStrings.Cancel
                && resultFromHistory != LocalizedStrings.ClearHistory)
                if (_nextStroke != NextInput.DoNothing)
                {
                    Input = resultFromHistory;
                    _nextStroke = NextInput.DoNothing;
                }
                else
                    Input += resultFromHistory;
            else if (resultFromHistory == LocalizedStrings.ClearHistory)
                Settings.Instance.ClearResultsHistory();
        }

        private async Task NavigateToSettingsAsync() =>
            await _navigationService.NavigateToAsync(Locations.SettingsPage);

        private async Task ShowAbout() =>
            await _alertsService.DisplayAlertAsync(
                LocalizedStrings.About,
                (_platformInformationService.PlatformSupportsGettingApplicationVersion() ?
                    LocalizedStrings.AppVersion
                        + Environment.NewLine
                        + _platformInformationService.GetApplicationVersion()
                        + Environment.NewLine
                        + Environment.NewLine :
                    string.Empty)
                + LocalizedStrings.AppIconAttribution);
    }
}
