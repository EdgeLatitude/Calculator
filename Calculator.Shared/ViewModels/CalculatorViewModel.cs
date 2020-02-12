using Calculator.Shared.Constants;
using Calculator.Shared.Localization;
using Calculator.Shared.Logic;
using Calculator.Shared.Models.Enums;
using Calculator.Shared.PlatformServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Calculator.Shared.ViewModels
{
    public class CalculatorViewModel : BaseViewModel
    {
        private readonly IAlertsService _alertsService;
        private readonly ICommandFactoryService _commandFactoryService;
        private readonly INavigationService _navigationService;

        private readonly Dictionary<char, double> _variableStorageValues
            = new Dictionary<char, double>();

        private NextInput _nextStroke = NextInput.DoNothing;

        public CalculatorViewModel(
            IAlertsService alertsService,
            ICommandFactoryService commandFactoryService,
            INavigationService navigationService)
        {
            _alertsService = alertsService;
            _commandFactoryService = commandFactoryService;
            _navigationService = navigationService;

            AC_Command = _commandFactoryService.Create(() => AC());
            DeleteCommand = _commandFactoryService.Create(() => Delete());
            BinaryOperatorCommand = _commandFactoryService.Create<string>((symbol) => BinaryOperator(symbol));
            UnaryOperatorCommand = _commandFactoryService.Create<string>((symbol) => UnaryOperator(symbol));
            ParenthesesCommand = _commandFactoryService.Create<string>((parentheses) => Parentheses(parentheses));
            LastResultCommand = _commandFactoryService.Create(() => LastResult());
            NumberCommand = _commandFactoryService.Create<string>((number) => Number(number));
            DecimalCommand = _commandFactoryService.Create(() => Decimal());
            CalculateCommand = _commandFactoryService.Create(() => Calculate());
            ShowHistoryCommand = _commandFactoryService.Create(() => ShowHistory());
            NavigateToSettingsCommand = _commandFactoryService.Create(async () => await NavigateToSettingsAsync());
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
            }
        }

        public string DecimalSeparator
        {
            get => Logic.Calculator.DecimalSeparator;
        }

        public ICommand AC_Command { get; private set; }

        public ICommand DeleteCommand { get; private set; }

        public ICommand BinaryOperatorCommand { get; private set; }

        public ICommand UnaryOperatorCommand { get; private set; }

        public ICommand ParenthesesCommand { get; private set; }

        public ICommand LastResultCommand { get; private set; }

        public ICommand NumberCommand { get; private set; }

        public ICommand DecimalCommand { get; private set; }

        public ICommand CalculateCommand { get; private set; }

        public ICommand ShowHistoryCommand { get; private set; }

        public ICommand NavigateToSettingsCommand { get; private set; }

        private void AC()
        {
            // Clear user input and memory values
            Input = string.Empty;
            _nextStroke = NextInput.DoNothing;
            _variableStorageValues.Clear();
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
            Input = Input.Substring(0, Input.Length - 1);
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
            // Calculate and show corresponding result
            var calculationResult = Logic.Calculator.Calculate(Input, _variableStorageValues);
            if (calculationResult != null)
                // Show result if calculation was successful
                if (calculationResult.Successful)
                {
                    var result = calculationResult.Result;

                    var resultText = result.ToString();
                    while (resultText.Contains(Logic.Calculator.DecimalSeparator)
                        && (char.ToString(resultText[resultText.Length - 1]) == Logic.Calculator.ZeroString
                            || char.ToString(resultText[resultText.Length - 1]) == Logic.Calculator.DecimalSeparator))
                        resultText = resultText.Substring(0, resultText.Length - 1);

                    Input = resultText;
                    _nextStroke = NextInput.ClearAtNumber;

                    AddOrUpdateVariableStorage(Logic.Calculator.LastResult, result);

                    _ = Settings.Instance.ManageNewResultAsync(resultText);
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
        }

        private void AddOrUpdateVariableStorage(char storage, double value)
        {
            // If memory value already exists, overwrite it, else, add it
            if (_variableStorageValues.TryGetValue(storage, out double _))
                _variableStorageValues[storage] = value;
            else
                _variableStorageValues.Add(storage, value);
        }

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
    }
}
