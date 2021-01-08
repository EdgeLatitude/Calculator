using Calculator.Shared.Constants;
using Calculator.Shared.Localization;
using Calculator.Shared.Logic;
using Calculator.Shared.Models.Enums;
using Calculator.Shared.PlatformServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Calculator.Shared.ViewModels
{
    public class CalculatorViewModel : BaseViewModel
    {
        private static readonly List<string> _possibleDecimalSeparators
            = new List<string>()
            {
                LexicalSymbols.Comma,
                LexicalSymbols.Dot
            };

        private static readonly Dictionary<string, string> _equivalentSymbols
            = new Dictionary<string, string>()
            {
                { LexicalSymbols.SimpleDivisionOperator, LexicalSymbols.DivisionOperator },
                { LexicalSymbols.SimpleMultiplicationOperator, LexicalSymbols.MultiplicationOperator }
            };

        private readonly IAlertsService _alertsService;
        private readonly IClipboardService _clipboardService;
        private readonly ICommandFactoryService _commandFactoryService;
        private readonly INavigationService _navigationService;
        private readonly IPlatformInformationService _platformInformationService;

        private readonly Dictionary<string, decimal> _variableStorageValues
            = new Dictionary<string, decimal>();

        private bool _calculating;

        private InputSectionViewModel[] _lastInput;

        private NextInput _nextStroke = NextInput.DoNothing;

        private InputSectionViewModel _selectedInputSection;

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
            DeleteCommand = _commandFactoryService.Create(async () => await Delete());
            BinaryOperatorCommand = _commandFactoryService.Create<string>(async (symbol) => await BinaryOperator(symbol));
            UnaryOperatorCommand = _commandFactoryService.Create<string>(async (symbol) => await UnaryOperator(symbol));
            ParenthesisCommand = _commandFactoryService.Create<string>(async (parenthesis) => await Parenthesis(parenthesis));
            VariableStorageCommand = _commandFactoryService.Create<string>(async (symbol) => await VariableStorage(symbol));
            NumberCommand = _commandFactoryService.Create<string>(async (number) => await Number(number));
            DecimalCommand = _commandFactoryService.Create(async () => await Decimal());
            CalculateCommand = _commandFactoryService.Create(async () => await Calculate());
            CopyInputToClipboardCommand = _commandFactoryService.Create(async () => await CopyInputToClipboard());
            SelectInputSectionCommand = _commandFactoryService.Create<InputSectionViewModel>(async (inputSectionViewModel) => await SelectInputSection(inputSectionViewModel));
            ManageInputFromHardwareCommand = _commandFactoryService.Create<string>(async (character) => await ManageInputFromHardware(character));
            ShowHistoryCommand = _commandFactoryService.Create(async () => await ShowHistory());
            NavigateToSettingsCommand = _commandFactoryService.Create(async () => await NavigateToSettingsAsync());
            ShowAboutCommand = _commandFactoryService.Create(async () => await ShowAbout());

            Input.CollectionChanged += Input_CollectionChanged;
        }

        public string DecimalSeparator => Logic.Calculator.DecimalSeparator;

        private readonly ObservableCollection<InputSectionViewModel> _input = new ObservableCollection<InputSectionViewModel>();

        public ObservableCollection<InputSectionViewModel> Input => _input;

        public bool AfterResult { get; private set; }

        public ICommand AllClearCommand { get; private set; }

        public ICommand ClearCommand { get; private set; }

        public ICommand DeleteCommand { get; private set; }

        public ICommand BinaryOperatorCommand { get; private set; }

        public ICommand UnaryOperatorCommand { get; private set; }

        public ICommand ParenthesisCommand { get; private set; }

        public ICommand VariableStorageCommand { get; private set; }

        public ICommand NumberCommand { get; private set; }

        public ICommand DecimalCommand { get; private set; }

        public ICommand CalculateCommand { get; private set; }

        public ICommand CopyInputToClipboardCommand { get; private set; }

        public ICommand SelectInputSectionCommand { get; private set; }

        public ICommand ManageInputFromHardwareCommand { get; private set; }

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
            Input.Clear();
            _nextStroke = NextInput.DoNothing;
        }

        private async Task Delete()
        {
            // Do nothing if there is currently no input
            if (!Input.Any())
                return;
            // Clear everything if required
            if (_nextStroke != NextInput.DoNothing)
            {
                Input.Clear();
                _nextStroke = NextInput.DoNothing;
                return;
            }
            // Else only delete 1 section, the selected one
            var indexOfSelectedInputSection = Input.IndexOf(_selectedInputSection);
            Input.RemoveAt(indexOfSelectedInputSection);
            _selectedInputSection = indexOfSelectedInputSection == 0 ?
                Input.Any() ?
                    Input.First() :
                    null :
                Input[indexOfSelectedInputSection - 1];
            if (_selectedInputSection != null)
                _selectedInputSection.IsSelected = true;

            await Task.Delay(100);
        }

        private async Task BinaryOperator(string symbol)
        {
            if (_nextStroke == NextInput.ClearAtAny)
            {
                ClearAndAddInputSection(symbol);
                _nextStroke = NextInput.DoNothing;
            }
            else if (_nextStroke == NextInput.ClearAtNumber)
            {
                ClearAndAddInputSection(LocalizedStrings.LastResultAbbreviation);
                await AddInputSection(symbol);
                _nextStroke = NextInput.DoNothing;
            }
            else
                await AddInputSection(symbol);
        }

        private async Task UnaryOperator(string symbol)
        {
            if (_nextStroke != NextInput.DoNothing)
            {
                ClearAndAddInputSection(symbol);
                _nextStroke = NextInput.DoNothing;
            }
            else
                await AddInputSection(symbol);
        }

        private async Task Parenthesis(string parenthesis)
        {
            if (_nextStroke != NextInput.DoNothing)
            {
                ClearAndAddInputSection(parenthesis);
                _nextStroke = NextInput.DoNothing;
            }
            else
                await AddInputSection(parenthesis);
        }

        private async Task VariableStorage(string symbol)
        {
            if (_nextStroke != NextInput.DoNothing)
            {
                ClearAndAddInputSection(symbol);
                _nextStroke = NextInput.DoNothing;
            }
            else
                await AddInputSection(symbol);
        }

        private async Task Number(string number)
        {
            if (_nextStroke == NextInput.ClearAtAny
                || _nextStroke == NextInput.ClearAtNumber)
            {
                ClearAndAddInputSection(number);
                _nextStroke = NextInput.DoNothing;
            }
            else
                await AddInputSection(number);
        }

        private async Task Decimal()
        {
            if (_nextStroke != NextInput.DoNothing)
            {
                ClearAndAddInputSection(DecimalSeparator);
                _nextStroke = NextInput.DoNothing;
            }
            else
                await AddInputSection(DecimalSeparator);
        }

        private async Task Calculate()
        {
            // Do nothing if there is no input
            if (!Input.Any())
                return;

            // Clear input if there was no interaction after an error
            if (_nextStroke == NextInput.ClearAtAny)
            {
                Input.Clear();
                _nextStroke = NextInput.DoNothing;
                return;
            }

            // Use previous input if it was a valid one and there was no interaction after calculating
            var input = _nextStroke == NextInput.ClearAtNumber ?
                _lastInput :
                Input.ToArray();

            // Calculate and show corresponding result
            _calculating = true;
            _lastInput = input;
            var calculationResult = await Logic.Calculator.CalculateAsync(JoinInputSectionsIntoSingleString(input), _variableStorageValues);
            if (calculationResult != null)
                // Show result if calculation was successful
                if (calculationResult.Successful)
                {
                    var result = calculationResult.Result;
                    if (TryFormatResult(result, out var resultText))
                    {
                        AfterResult = true;

                        ClearAndAddInputSection(resultText);
                        _nextStroke = NextInput.ClearAtNumber;

                        AddOrUpdateVariableStorage(LocalizedStrings.LastResultAbbreviation, result);

                        _ = Settings.Instance.ManageNewResultAsync(resultText);
                    }
                    // Show error message if result could not be formatted
                    else
                    {
                        ClearAndAddInputSection(LocalizedStrings.CalculationError);
                        _nextStroke = NextInput.ClearAtAny;
                    }

                }
                // Show error message if calculation was not successful
                else
                {
                    ClearAndAddInputSection(calculationResult.ErrorMessage);
                    _nextStroke = NextInput.ClearAtAny;
                }
            // It has no reason to be a null object
            else
            {
                ClearAndAddInputSection(LocalizedStrings.UnexpectedError);
                _nextStroke = NextInput.ClearAtAny;
            }
            _calculating = false;
        }

        private void AddOrUpdateVariableStorage(string storage, decimal value)
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
            while (resultText.Contains(DecimalSeparator)
                && (char.ToString(resultText[^1]) == Logic.Calculator.ZeroString
                    || char.ToString(resultText[^1]) == DecimalSeparator))
                resultText = resultText[0..^1];
            return true;
        }

        private async Task CopyInputToClipboard() =>
            await _clipboardService.SetTextAsync(JoinInputSectionsIntoSingleString(Input.ToArray()));

        private async Task SelectInputSection(InputSectionViewModel inputSectionViewModel)
        {
            if (_selectedInputSection != null)
                _selectedInputSection.IsSelected = false;
            _selectedInputSection = inputSectionViewModel;
            inputSectionViewModel.IsSelected = true;
            await Task.Delay(100);
        }

        private async Task ManageInputFromHardware(string character)
        {
            if (Logic.Calculator.VariableStorageWords.Contains(character))
                await VariableStorage(character);
            else if (Logic.Calculator.Parentheses.Contains(character))
                await Parenthesis(character);
            else if (Logic.Calculator.BinaryOperators.Contains(character))
                await BinaryOperator(character);
            else if (Logic.Calculator.UnaryOperators.Contains(character))
                await UnaryOperator(character);
            else if (Logic.Calculator.Numbers.Contains(character))
                await Number(character);
            else if (_possibleDecimalSeparators.Contains(character))
                await Decimal();
            else if (_equivalentSymbols.ContainsKey(character))
                await ManageInputFromHardware(_equivalentSymbols[character]);
        }

        private async Task ShowHistory()
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
                    ClearAndAddInputSection(resultFromHistory);
                    _nextStroke = NextInput.DoNothing;
                }
                else
                    await AddInputSection(resultFromHistory);
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

        private void ClearAndAddInputSection(string input)
        {
            Input.Clear();
            var onlySection = new InputSectionViewModel(input);
            Input.Add(onlySection);
            _selectedInputSection = onlySection;
        }

        private async Task AddInputSection(string input)
        {
            if (_selectedInputSection == null)
            {
                var onlySection = new InputSectionViewModel(input);
                Input.Add(onlySection);
                _selectedInputSection = onlySection;
                return;
            }

            var indexOfSelectedInputSection = Input.IndexOf(_selectedInputSection);
            _selectedInputSection.IsSelected = false;

            var newSection = new InputSectionViewModel(input);
            Input.Insert(indexOfSelectedInputSection + 1, newSection);
            _selectedInputSection = newSection;

            await Task.Delay(100);
        }

        private string JoinInputSectionsIntoSingleString(InputSectionViewModel[] input) =>
            string.Join(string.Empty, input.Select(inputSection => inputSection.Input));

        private void Input_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (!_calculating)
                AfterResult = false;
        }
    }
}
