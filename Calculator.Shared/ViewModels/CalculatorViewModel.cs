using Calculator.Shared.Constants;
using Calculator.Shared.Extensions;
using Calculator.Shared.Localization;
using Calculator.Shared.Logic;
using Calculator.Shared.Models.Enums;
using Calculator.Shared.PlatformServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Calculator.Shared.ViewModels
{
    public class CalculatorViewModel : BaseViewModel
    {
        #region Fields
        private bool _isCalculating;
        private bool _isPasting;

        private NextInput _nextStroke = NextInput.DoNothing;
        private InputSectionViewModel _selectedInputSection;
        private InputSectionViewModel[] _lastInput;

        private readonly Logic.Calculator _calculator;
        private readonly Settings _settings;

        private readonly IAlertsService _alertsService;
        private readonly IClipboardService _clipboardService;
        private readonly INavigationService _navigationService;

        private readonly IReadOnlyDictionary<string, string> _equivalentSymbols
            = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()
            {
                { LexicalSymbols.SimpleDivisionOperator, LexicalSymbols.DivisionOperator },
                { LexicalSymbols.SimpleMultiplicationOperator, LexicalSymbols.MultiplicationOperator }
            });

        private readonly ReadOnlyCollection<string> _possibleDecimalSeparators
            = new ReadOnlyCollection<string>(new string[]
            {
                LexicalSymbols.Comma,
                LexicalSymbols.Dot
            });

        private readonly IDictionary<string, decimal> _variableStorageValues
            = new Dictionary<string, decimal>();
        #endregion

        #region Properties
        private CultureInfo CurrentCulture =>
            Thread.CurrentThread.CurrentCulture;

        public string DecimalSeparator =>
            CurrentCulture.NumberFormat.NumberDecimalSeparator;

        public string GroupSeparator =>
            CurrentCulture.NumberFormat.NumberGroupSeparator;

        private readonly ObservableCollection<InputSectionViewModel> _input = new ObservableCollection<InputSectionViewModel>();

        public ObservableCollection<InputSectionViewModel> Input => _input;

        public bool AfterResult { get; private set; }
        #endregion

        #region Commands
        public ICommand AllClearCommand { get; }

        public ICommand ClearCommand { get; }

        public ICommand DeleteCommand { get; }

        public ICommand BinaryOperatorCommand { get; }

        public ICommand UnaryOperatorCommand { get; }

        public ICommand ParenthesisCommand { get; }

        public ICommand VariableStorageCommand { get; }

        public ICommand NumberCommand { get; }

        public ICommand DecimalCommand { get; }

        public ICommand CalculateCommand { get; }

        public ICommand CopyCommand { get; }

        public ICommand PasteCommand { get; }

        public ICommand SelectInputSectionCommand { get; }

        public ICommand ManageInputCharacterCommand { get; }

        public ICommand ShowHistoryCommand { get; }

        public ICommand NavigateToSettingsCommand { get; }

        public ICommand NavigateToAboutCommand { get; }
        #endregion

        #region Constructors
        public CalculatorViewModel(
            Logic.Calculator calculator,
            Settings settings,
            IAlertsService alertsService,
            IClipboardService clipboardService,
            ICommandFactoryService commandFactoryService,
            INavigationService navigationService)
        {
            _calculator = calculator;
            _settings = settings;

            _alertsService = alertsService;
            _clipboardService = clipboardService;
            _navigationService = navigationService;

            AllClearCommand = commandFactoryService.Create(AllClear);
            ClearCommand = commandFactoryService.Create(Clear);
            DeleteCommand = commandFactoryService.Create(async () => await DeleteAsync());
            BinaryOperatorCommand = commandFactoryService.Create<string>(async (symbol) => await BinaryOperatorAsync(symbol, true));
            UnaryOperatorCommand = commandFactoryService.Create<string>(async (symbol) => await UnaryOperatorAsync(symbol, true));
            ParenthesisCommand = commandFactoryService.Create<string>(async (parenthesis) => await ParenthesisAsync(parenthesis, true));
            VariableStorageCommand = commandFactoryService.Create<string>(async (symbol) => await VariableStorageAsync(symbol, true));
            NumberCommand = commandFactoryService.Create<string>(async (number) => await NumberAsync(number, true));
            DecimalCommand = commandFactoryService.Create(async () => await DecimalAsync(true));
            CalculateCommand = commandFactoryService.Create(async () => await CalculateAsync());
            CopyCommand = commandFactoryService.Create(async () => await CopyAsync());
            PasteCommand = commandFactoryService.Create(async () => await PasteAsync());
            SelectInputSectionCommand = commandFactoryService.Create<InputSectionViewModel>(async (inputSectionViewModel) => await SelectInputSectionAsync(inputSectionViewModel));
            ManageInputCharacterCommand = commandFactoryService.Create<string>(async (character) => await ManageInputCharacterAsync(character, true));
            ShowHistoryCommand = commandFactoryService.Create(async () => await ShowHistoryAsync());
            NavigateToSettingsCommand = commandFactoryService.Create(async () => await NavigateToSettingsAsync());
            NavigateToAboutCommand = commandFactoryService.Create(async () => await NavigateToAboutAsync());

            Input.CollectionChanged += Input_CollectionChanged;
        }

        ~CalculatorViewModel() =>
            Input.CollectionChanged -= Input_CollectionChanged;
        #endregion

        #region Methods
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

        private async Task DeleteAsync()
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

            // Prevent UI glitches.
            await UiDelayAsync();
        }

        private async Task BinaryOperatorAsync(string symbol, bool uiDelay)
        {
            if (_nextStroke == NextInput.ClearAtAny)
            {
                ClearAndAddInputSection(symbol);
                _nextStroke = NextInput.DoNothing;
            }
            else if (_nextStroke == NextInput.ClearAtNumber)
            {
                ClearAndAddInputSection(LocalizedStrings.LastResultAbbreviation);
                await AddInputSectionAsync(symbol, uiDelay);
                _nextStroke = NextInput.DoNothing;
            }
            else
                await AddInputSectionAsync(symbol, uiDelay);
        }

        private async Task UnaryOperatorAsync(string symbol, bool uiDelay)
        {
            if (_nextStroke != NextInput.DoNothing)
            {
                ClearAndAddInputSection(symbol);
                _nextStroke = NextInput.DoNothing;
            }
            else
                await AddInputSectionAsync(symbol, uiDelay);
        }

        private async Task ParenthesisAsync(string parenthesis, bool uiDelay)
        {
            if (_nextStroke != NextInput.DoNothing)
            {
                ClearAndAddInputSection(parenthesis);
                _nextStroke = NextInput.DoNothing;
            }
            else
                await AddInputSectionAsync(parenthesis, uiDelay);
        }

        private async Task VariableStorageAsync(string symbol, bool uiDelay)
        {
            if (_nextStroke != NextInput.DoNothing)
            {
                ClearAndAddInputSection(symbol);
                _nextStroke = NextInput.DoNothing;
            }
            else
                await AddInputSectionAsync(symbol, uiDelay);
        }

        private async Task NumberAsync(string number, bool uiDelay)
        {
            if (_nextStroke == NextInput.ClearAtAny
                || _nextStroke == NextInput.ClearAtNumber)
            {
                ClearAndAddInputSection(number);
                _nextStroke = NextInput.DoNothing;
            }
            else
                await AddInputSectionAsync(number, uiDelay);
        }

        private async Task DecimalAsync(bool uiDelay)
        {
            if (_nextStroke != NextInput.DoNothing)
            {
                ClearAndAddInputSection(DecimalSeparator);
                _nextStroke = NextInput.DoNothing;
            }
            else
                await AddInputSectionAsync(DecimalSeparator, uiDelay);
        }

        private async Task CalculateAsync()
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
            _isCalculating = true;
            _lastInput = input;
            var calculationResult = await _calculator.CalculateAsync(JoinInputSectionsIntoSingleString(input), DecimalSeparator, _variableStorageValues);
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

                        _settings.ManageNewResultAsync(RemoveGroupsSeparatorFromText(resultText)).AwaitInOtherContext(true);
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
            _isCalculating = false;
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
            resultText = result.ToString("N", CurrentCulture);
            while (resultText.Contains(DecimalSeparator)
                && (char.ToString(resultText[^1]) == _calculator.ZeroString
                    || char.ToString(resultText[^1]) == DecimalSeparator))
                resultText = resultText[0..^1];
            return true;
        }

        private string RemoveGroupsSeparatorFromText(string text) =>
            text.Replace(GroupSeparator, string.Empty);

        private Task CopyAsync() =>
            _clipboardService.SetTextAsync(RemoveGroupsSeparatorFromText(JoinInputSectionsIntoSingleString(Input.ToArray())));

        private async Task PasteAsync()
        {
            if (_isPasting)
                return;

            _isPasting = true;

            var clipboardText = await _clipboardService.GetTextAsync();
            if (string.IsNullOrWhiteSpace(clipboardText))
            {
                await _alertsService.DisplayAlertAsync(
                    LocalizedStrings.Notice,
                    LocalizedStrings.TheClipboardIsEmpty);
                _isPasting = false;
                return;
            }

            clipboardText = clipboardText.Replace(Strings.WhiteSpace, string.Empty);
            foreach (var symbol in clipboardText)
                if (!await ManageInputCharacterAsync(char.ToString(symbol), false))
                {
                    await _alertsService.DisplayAlertAsync(
                        LocalizedStrings.Notice,
                        LocalizedStrings.YouCanOnlyPasteValidNumbersOrOperations);
                    _isPasting = false;
                    return;
                }

            _isPasting = false;
        }

        private Task SelectInputSectionAsync(InputSectionViewModel inputSectionViewModel)
        {
            if (_selectedInputSection != null)
                _selectedInputSection.IsSelected = false;
            _selectedInputSection = inputSectionViewModel;
            inputSectionViewModel.IsSelected = true;
            return UiDelayAsync();
        }

        private async Task<bool> ManageInputCharacterAsync(string character, bool uiDelay)
        {
            if (_calculator.VariableStorageWords.Contains(character))
            {
                await VariableStorageAsync(character, uiDelay);
                return true;
            }
            else if (_calculator.Parentheses.Contains(character))
            {
                await ParenthesisAsync(character, uiDelay);
                return true;
            }
            else if (_calculator.BinaryOperators.Contains(character))
            {
                await BinaryOperatorAsync(character, uiDelay);
                return true;
            }
            else if (_calculator.UnaryOperators.Contains(character))
            {
                await UnaryOperatorAsync(character, uiDelay);
                return true;
            }
            else if (_calculator.Numbers.Contains(character))
            {
                await NumberAsync(character, uiDelay);
                return true;
            }
            else if (_possibleDecimalSeparators.Contains(character))
            {
                await DecimalAsync(uiDelay);
                return true;
            }
            else if (_equivalentSymbols.ContainsKey(character))
                return await ManageInputCharacterAsync(_equivalentSymbols[character], uiDelay);
            else
                return false;
        }

        private async Task ShowHistoryAsync()
        {
            if (_settings.GetResultsHistoryLength() == 0)
            {
                var openSettings = await _alertsService.DisplayConfirmationAsync(LocalizedStrings.Notice,
                    LocalizedStrings.DisabledResultsHistory,
                    LocalizedStrings.Settings);
                if (openSettings)
                    await NavigateToSettingsAsync();
                return;
            }

            if (!_settings.ContainsResultsHistory())
            {
                await _alertsService.DisplayAlertAsync(
                    LocalizedStrings.Notice,
                    LocalizedStrings.EmptyResultsHistory);
                return;
            }

            var resultsHistory = await _settings.GetResultsHistoryAsync();
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
                    await AddInputSectionAsync(resultFromHistory, true);
            else if (resultFromHistory == LocalizedStrings.ClearHistory)
                _settings.ClearResultsHistory();
        }

        private Task NavigateToSettingsAsync() =>
            _navigationService.NavigateToAsync(Locations.SettingsPage);

        private Task NavigateToAboutAsync() =>
            _navigationService.NavigateToAsync(Locations.AboutPage);

        private void ClearAndAddInputSection(string input)
        {
            Input.Clear();
            var onlySection = new InputSectionViewModel(input);
            Input.Add(onlySection);
            _selectedInputSection = onlySection;
        }

        private async Task AddInputSectionAsync(string input, bool uiDelay)
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

            if (uiDelay)
                await UiDelayAsync();
        }

        private Task UiDelayAsync() =>
            Task.Delay(100);

        private string JoinInputSectionsIntoSingleString(InputSectionViewModel[] input) =>
            string.Join(string.Empty, input.Select(inputSection => inputSection.Input));

        private void Input_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (!_isCalculating)
                AfterResult = false;
        }
        #endregion
    }
}
