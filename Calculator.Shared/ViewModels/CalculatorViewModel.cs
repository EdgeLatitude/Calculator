﻿using Calculator.Shared.Constants;
using Calculator.Shared.Localization;
using Calculator.Shared.Logic;
using Calculator.Shared.Models.Enums;
using Calculator.Shared.PlatformServices;
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
        private readonly ICommandFactoryService _commandFactoryService;
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
        public string DecimalSeparator => _calculator.DecimalSeparator;

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
            _commandFactoryService = commandFactoryService;
            _navigationService = navigationService;

            AllClearCommand = _commandFactoryService.Create(AllClear);
            ClearCommand = _commandFactoryService.Create(Clear);
            DeleteCommand = _commandFactoryService.Create(async () => await DeleteAsync());
            BinaryOperatorCommand = _commandFactoryService.Create<string>(async (symbol) => await BinaryOperatorAsync(symbol));
            UnaryOperatorCommand = _commandFactoryService.Create<string>(async (symbol) => await UnaryOperatorAsync(symbol));
            ParenthesisCommand = _commandFactoryService.Create<string>(async (parenthesis) => await ParenthesisAsync(parenthesis));
            VariableStorageCommand = _commandFactoryService.Create<string>(async (symbol) => await VariableStorageAsync(symbol));
            NumberCommand = _commandFactoryService.Create<string>(async (number) => await NumberAsync(number));
            DecimalCommand = _commandFactoryService.Create(async () => await DecimalAsync());
            CalculateCommand = _commandFactoryService.Create(async () => await CalculateAsync());
            CopyCommand = _commandFactoryService.Create(async () => await CopyAsync());
            PasteCommand = _commandFactoryService.Create(async () => await PasteAsync());
            SelectInputSectionCommand = _commandFactoryService.Create<InputSectionViewModel>(async (inputSectionViewModel) => await SelectInputSectionAsync(inputSectionViewModel));
            ManageInputCharacterCommand = _commandFactoryService.Create<string>(async (character) => await ManageInputCharacterAsync(character));
            ShowHistoryCommand = _commandFactoryService.Create(async () => await ShowHistoryAsync());
            NavigateToSettingsCommand = _commandFactoryService.Create(async () => await NavigateToSettingsAsync());
            NavigateToAboutCommand = _commandFactoryService.Create(async () => await NavigateToAboutAsync());

            Input.CollectionChanged += Input_CollectionChanged;
        }
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
            await Task.Delay(100);
        }

        private async Task BinaryOperatorAsync(string symbol)
        {
            if (_nextStroke == NextInput.ClearAtAny)
            {
                ClearAndAddInputSection(symbol);
                _nextStroke = NextInput.DoNothing;
            }
            else if (_nextStroke == NextInput.ClearAtNumber)
            {
                ClearAndAddInputSection(LocalizedStrings.LastResultAbbreviation);
                await AddInputSectionAsync(symbol);
                _nextStroke = NextInput.DoNothing;
            }
            else
                await AddInputSectionAsync(symbol);
        }

        private async Task UnaryOperatorAsync(string symbol)
        {
            if (_nextStroke != NextInput.DoNothing)
            {
                ClearAndAddInputSection(symbol);
                _nextStroke = NextInput.DoNothing;
            }
            else
                await AddInputSectionAsync(symbol);
        }

        private async Task ParenthesisAsync(string parenthesis)
        {
            if (_nextStroke != NextInput.DoNothing)
            {
                ClearAndAddInputSection(parenthesis);
                _nextStroke = NextInput.DoNothing;
            }
            else
                await AddInputSectionAsync(parenthesis);
        }

        private async Task VariableStorageAsync(string symbol)
        {
            if (_nextStroke != NextInput.DoNothing)
            {
                ClearAndAddInputSection(symbol);
                _nextStroke = NextInput.DoNothing;
            }
            else
                await AddInputSectionAsync(symbol);
        }

        private async Task NumberAsync(string number)
        {
            if (_nextStroke == NextInput.ClearAtAny
                || _nextStroke == NextInput.ClearAtNumber)
            {
                ClearAndAddInputSection(number);
                _nextStroke = NextInput.DoNothing;
            }
            else
                await AddInputSectionAsync(number);
        }

        private async Task DecimalAsync()
        {
            if (_nextStroke != NextInput.DoNothing)
            {
                ClearAndAddInputSection(DecimalSeparator);
                _nextStroke = NextInput.DoNothing;
            }
            else
                await AddInputSectionAsync(DecimalSeparator);
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
            var calculationResult = await _calculator.CalculateAsync(JoinInputSectionsIntoSingleString(input), _variableStorageValues);
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

                        _ = _settings.ManageNewResultAsync(resultText);
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
            resultText = result.ToString();
            while (resultText.Contains(DecimalSeparator)
                && (char.ToString(resultText[^1]) == _calculator.ZeroString
                    || char.ToString(resultText[^1]) == DecimalSeparator))
                resultText = resultText[0..^1];
            return true;
        }

        private async Task CopyAsync() =>
            await _clipboardService.SetTextAsync(JoinInputSectionsIntoSingleString(Input.ToArray()));

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
                if (!await ManageInputCharacterAsync(char.ToString(symbol)))
                {
                    await _alertsService.DisplayAlertAsync(
                        LocalizedStrings.Notice,
                        LocalizedStrings.YouCanOnlyPasteValidNumbersOrOperations);
                    _isPasting = false;
                    return;
                }

            _isPasting = false;
        }

        private async Task SelectInputSectionAsync(InputSectionViewModel inputSectionViewModel)
        {
            if (_selectedInputSection != null)
                _selectedInputSection.IsSelected = false;
            _selectedInputSection = inputSectionViewModel;
            inputSectionViewModel.IsSelected = true;
            await Task.Delay(100);
        }

        private async Task<bool> ManageInputCharacterAsync(string character)
        {
            if (_calculator.VariableStorageWords.Contains(character))
            {
                await VariableStorageAsync(character);
                return true;
            }
            else if (_calculator.Parentheses.Contains(character))
            {
                await ParenthesisAsync(character);
                return true;
            }
            else if (_calculator.BinaryOperators.Contains(character))
            {
                await BinaryOperatorAsync(character);
                return true;
            }
            else if (_calculator.UnaryOperators.Contains(character))
            {
                await UnaryOperatorAsync(character);
                return true;
            }
            else if (_calculator.Numbers.Contains(character))
            {
                await NumberAsync(character);
                return true;
            }
            else if (_possibleDecimalSeparators.Contains(character))
            {
                await DecimalAsync();
                return true;
            }
            else if (_equivalentSymbols.ContainsKey(character))
                return await ManageInputCharacterAsync(_equivalentSymbols[character]);
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
                    await AddInputSectionAsync(resultFromHistory);
            else if (resultFromHistory == LocalizedStrings.ClearHistory)
                _settings.ClearResultsHistory();
        }

        private async Task NavigateToSettingsAsync() =>
            await _navigationService.NavigateToAsync(Locations.SettingsPage);

        private async Task NavigateToAboutAsync() =>
            await _navigationService.NavigateToAsync(Locations.AboutPage);

        private void ClearAndAddInputSection(string input)
        {
            Input.Clear();
            var onlySection = new InputSectionViewModel(input);
            Input.Add(onlySection);
            _selectedInputSection = onlySection;
        }

        private async Task AddInputSectionAsync(string input)
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
            if (!_isCalculating)
                AfterResult = false;
        }
        #endregion
    }
}
