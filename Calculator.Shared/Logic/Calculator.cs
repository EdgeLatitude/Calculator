using Calculator.Shared.Constants;
using Calculator.Shared.Localization;
using Calculator.Shared.Models.Enums;
using Calculator.Shared.Models.MathObjects;
using Calculator.Shared.Models.Results;
using DecimalMath;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Calculator.Shared.Logic
{
    public class Calculator
    {
        public Calculator()
        {
            // Initialize separators array from list filled with specified lexical collections
            var separators = new List<string>();
            separators.AddRange(VariableStorageWords);
            separators.AddRange(Parentheses);
            separators.AddRange(BinaryOperators);
            separators.AddRange(UnaryOperators);
            _separators = separators.AsReadOnly();
        }

        // Compilation constants
        public const string WhiteSpace = " ";

        // Runtime constants
        public readonly string ZeroString = decimal.Zero.ToString();
        public readonly string MinusOneString = decimal.MinusOne.ToString();
        public readonly string DecimalSeparator
            = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        #region General definitions
        private readonly IReadOnlyDictionary<TerminalSymbol, string> _terminalSymbolsVariableStorageCharacter
            = new ReadOnlyDictionary<TerminalSymbol, string>(new Dictionary<TerminalSymbol, string>
        {
            { TerminalSymbol.LastResult, LocalizedStrings.LastResultAbbreviation }
        });
        #endregion

        #region Lexical definitions
        // Lexical collections
        public readonly ReadOnlyCollection<string> VariableStorageWords
            = new ReadOnlyCollection<string>(new string[] { LocalizedStrings.LastResultAbbreviation }); // 1 terminal symbol for each
        public readonly ReadOnlyCollection<string> Parentheses
            = new ReadOnlyCollection<string>(new string[] { LexicalSymbols.OpeningParenthesis, LexicalSymbols.ClosingParenthesis }); // 1 terminal symbol for each
        public readonly ReadOnlyCollection<string> BinaryOperators
            = new ReadOnlyCollection<string>(new string[] { LexicalSymbols.AdditionOperator, LexicalSymbols.SubstractionOperator, LexicalSymbols.MultiplicationOperator, LexicalSymbols.DivisionOperator, LexicalSymbols.PotentiationOperator }); // 1 terminal symbol for each
        public readonly ReadOnlyCollection<string> UnaryOperators
            = new ReadOnlyCollection<string>(new string[] { LexicalSymbols.SquareRootOperator }); // 1 terminal symbol for each
        public readonly ReadOnlyCollection<string> Numbers
            = new ReadOnlyCollection<string>(new string[] { LexicalSymbols.Zero, LexicalSymbols.One, LexicalSymbols.Two, LexicalSymbols.Three, LexicalSymbols.Four, LexicalSymbols.Five, LexicalSymbols.Six, LexicalSymbols.Seven, LexicalSymbols.Eight, LexicalSymbols.Nine }); // 1 terminal symbol for a complete real number

        // Separators array to be filled with some lexical collections
        private readonly ReadOnlyCollection<string> _separators;

        // Automata for lexical analysis
        private readonly int?[,] _automata = new int?[,]
        {           //  Numbers     Decimal
        /* S0 */    {   2,          1,      }, // Initial state
        /* S1 */    {   3,          null,   },
        /* S2 */    {   2,          1,      }, // Accepted state ✓
        /* S3 */    {   3,          null,   }  // Accepted state ✓
        };
        #endregion

        #region Syntax definitions
        // Terminal symbols groups
        private readonly IReadOnlyDictionary<TerminalSymbol, TerminalSymbolGroup> _terminalSymbolsGroups
            = new ReadOnlyDictionary<TerminalSymbol, TerminalSymbolGroup>(new Dictionary<TerminalSymbol, TerminalSymbolGroup>
        {
            { TerminalSymbol.None, TerminalSymbolGroup.None },
            { TerminalSymbol.LastResult, TerminalSymbolGroup.VariableStorageCharacters },
            { TerminalSymbol.OpeningParenthesis, TerminalSymbolGroup.Parentheses },
            { TerminalSymbol.ClosingParenthesis, TerminalSymbolGroup.Parentheses },
            { TerminalSymbol.AdditionOperator, TerminalSymbolGroup.BinaryOperators },
            { TerminalSymbol.SubstractionOperator, TerminalSymbolGroup.BinaryOperators },
            { TerminalSymbol.MultiplicationOperator, TerminalSymbolGroup.BinaryOperators },
            { TerminalSymbol.DivisionOperator, TerminalSymbolGroup.BinaryOperators },
            { TerminalSymbol.PotentiationOperator, TerminalSymbolGroup.BinaryOperators },
            { TerminalSymbol.SquareRootOperator, TerminalSymbolGroup.UnaryOperators },
            { TerminalSymbol.RealNumber, TerminalSymbolGroup.None },
            { TerminalSymbol.Nothing, TerminalSymbolGroup.None },
            { TerminalSymbol.OperandNegatorOperator, TerminalSymbolGroup.BinaryOperators }
        });

        // Grammar productions for each non terminal symbol
        private readonly IReadOnlyDictionary<NonTerminalSymbol, Enum[][]> _grammarNonTerminalSymbols
            = new ReadOnlyDictionary<NonTerminalSymbol, Enum[][]>(new Dictionary<NonTerminalSymbol, Enum[][]>
        {
            { NonTerminalSymbol.Expression, new Enum[][]
                {
                    new Enum[] { NonTerminalSymbol.Operand, NonTerminalSymbol.BinaryOperator },
                    new Enum[] { TerminalSymbolGroup.UnaryOperators, NonTerminalSymbol.Expression },
                    new Enum[] { TerminalSymbol.OpeningParenthesis, NonTerminalSymbol.Expression, TerminalSymbol.ClosingParenthesis, NonTerminalSymbol.BinaryOperator }
                }},
            { NonTerminalSymbol.Operand, new Enum[][]
                {
                    new Enum[] { TerminalSymbol.RealNumber, NonTerminalSymbol.MemoryOption },
                    new Enum[] { TerminalSymbolGroup.VariableStorageCharacters, NonTerminalSymbol.MemoryOption }
                }},
            { NonTerminalSymbol.MemoryOption, new Enum[][]
                {
                    new Enum[] { TerminalSymbolGroup.VariableStorageCharacters, NonTerminalSymbol.MemoryOption },
                    new Enum[] { }
                }},
            { NonTerminalSymbol.BinaryOperator, new Enum[][]
                {
                    new Enum[] { TerminalSymbolGroup.BinaryOperators, NonTerminalSymbol.Expression },
                    new Enum[] { }
                }}
        });

        // Predictive table for each non terminal symbol
        private readonly IReadOnlyDictionary<NonTerminalSymbol, int?[]> _predictiveTable
            = new ReadOnlyDictionary<NonTerminalSymbol, int?[]>(new Dictionary<NonTerminalSymbol, int?[]>
        {
            { NonTerminalSymbol.Expression,
                new int?[] { 0000, 0002, null, null, 0001, 0000, null } },
            { NonTerminalSymbol.Operand,
                new int?[] { 0001, null, null, null, null, 0000, null } },
            { NonTerminalSymbol.MemoryOption,
                new int?[] { 0000, null, 0001, 0001, null, null, 0001 } },
            { NonTerminalSymbol.BinaryOperator,
                new int?[] { null, null, 0001, 0000, null, null, 0001 } }
        });

        // Predictive table column definitions
        private readonly ReadOnlyCollection<Enum> _predictiveTableColumns = new ReadOnlyCollection<Enum>(new Enum[]
        {
            TerminalSymbolGroup.VariableStorageCharacters,
            TerminalSymbol.OpeningParenthesis,
            TerminalSymbol.ClosingParenthesis,
            TerminalSymbolGroup.BinaryOperators,
            TerminalSymbolGroup.UnaryOperators,
            TerminalSymbol.RealNumber,
            TerminalSymbol.Nothing
        });

        // Starting non terminal symbol
        private const NonTerminalSymbol _startingNonTerminalSymbol = NonTerminalSymbol.Expression;
        #endregion

        #region Semantic definitions
        // Operators dictionary by token
        private readonly IReadOnlyDictionary<TerminalSymbol, Operator> _operators
            = new ReadOnlyDictionary<TerminalSymbol, Operator>(new Dictionary<TerminalSymbol, Operator>
        {
            { TerminalSymbol.OpeningParenthesis, new Operator(TerminalSymbol.OpeningParenthesis, 0, LexicalSymbols.OpeningParenthesis) },
            { TerminalSymbol.ClosingParenthesis, new Operator(TerminalSymbol.ClosingParenthesis, 0, LexicalSymbols.ClosingParenthesis) },
            { TerminalSymbol.AdditionOperator, new Operator(TerminalSymbol.AdditionOperator, 1, LexicalSymbols.AdditionOperator, false, false) },
            { TerminalSymbol.SubstractionOperator, new Operator(TerminalSymbol.SubstractionOperator, 1, LexicalSymbols.SubstractionOperator, false, false) },
            { TerminalSymbol.MultiplicationOperator, new Operator(TerminalSymbol.MultiplicationOperator, 2, LexicalSymbols.MultiplicationOperator, false, false) },
            { TerminalSymbol.DivisionOperator, new Operator(TerminalSymbol.DivisionOperator, 2, LexicalSymbols.DivisionOperator, false, false) },
            { TerminalSymbol.PotentiationOperator, new Operator(TerminalSymbol.PotentiationOperator, 3, LexicalSymbols.PotentiationOperator, false, false) },
            { TerminalSymbol.SquareRootOperator, new Operator(TerminalSymbol.SquareRootOperator, 3, LexicalSymbols.SquareRootOperator, false, true) },
            { TerminalSymbol.OperandNegatorOperator, new Operator(TerminalSymbol.MultiplicationOperator, int.MaxValue, LexicalSymbols.MultiplicationOperator, false, false) }
        });
        #endregion

        internal async Task<CalculationResult> CalculateAsync(string operation, IDictionary<string, decimal> variableStorageValues) =>
            await Task.Run(() => Calculate(operation, variableStorageValues)).ConfigureAwait(false);

        private CalculationResult Calculate(string operation, IDictionary<string, decimal> variableStorageValues)
        {
            try
            {
                // Get operation components by properly splitting it
                var lexemes = SplitOperation(operation);
                if (lexemes == null)
                    return new CalculationResult(LocalizedStrings.UnexpectedError);
                // Lexical analysis
                var lexicalAnalysisResult = LexicalAnalysis(lexemes);
                if (lexicalAnalysisResult == null)
                    return new CalculationResult(LocalizedStrings.LexicalError);
                // Add possible missing join symbols to the operation
                lexicalAnalysisResult = AddMissingSymbols(lexicalAnalysisResult);
                // Syntax analysis
                var syntaxAndSemanticAnalysisResult = SyntaxAndSemanticAnalysis(variableStorageValues, lexicalAnalysisResult);
                if (syntaxAndSemanticAnalysisResult == null)
                    return new CalculationResult(LocalizedStrings.SyntaxError);
                // Do and return actual calculation
                return ActualCalculation(syntaxAndSemanticAnalysisResult);
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Error: " + exception.Message);
                return new CalculationResult(LocalizedStrings.UnexpectedError);
            }
        }

        private string[] SplitOperation(string operation)
        {
            // Add surrounding blank spaces to every separator in the operation
            foreach (var separator in _separators)
                if (operation.Contains(separator))
                    operation = operation.Replace(separator, WhiteSpace + separator + WhiteSpace);
            // Remove unnecessary blank spaces
            var doubleWhiteSpace = WhiteSpace + WhiteSpace;
            while (operation.Contains(doubleWhiteSpace))
                operation = operation.Replace(doubleWhiteSpace, WhiteSpace);
            operation = operation.Trim();
            // Split into the lexemes array using blank spaces
            return operation.Split();
        }

        private LexicalAnalysisResult LexicalAnalysis(string[] lexemes)
        {
            var terminalSymbols = new List<TerminalSymbol>();
            // Use the lexemes array to fill corresponding terminal symbols list
            foreach (var lexeme in lexemes)
            {
                var lexemeResult = AnalyzeLexeme(lexeme);
                if (lexemeResult?.Successful == true)
                    terminalSymbols.Add(lexemeResult.TerminalSymbol);
                else
                    return null;
            }
            return new LexicalAnalysisResult(lexemes, terminalSymbols.ToArray());
        }

        private LexemeResult AnalyzeLexeme(string lexeme)
        {
            var terminalSymbol = 0;
            // Check for lexeme in each lexical collection, at the end parse accumulated terminal symbol index into an actual symbol
            // Check in variable storage characters collection
            if (VariableStorageWords.Contains(lexeme))
                return new LexemeResult((TerminalSymbol)VariableStorageWords.IndexOf(lexeme) + terminalSymbol + 1);
            terminalSymbol += VariableStorageWords.Count;
            // Check in special characters and operators collections
            if (lexeme.Length == 1)
            {
                // Check in special characters collection
                if (Parentheses.Contains(lexeme))
                    return new LexemeResult((TerminalSymbol)Parentheses.IndexOf(lexeme) + terminalSymbol + 1);
                terminalSymbol += Parentheses.Count;
                // Check in binary operators collection
                if (BinaryOperators.Contains(lexeme))
                    return new LexemeResult((TerminalSymbol)BinaryOperators.IndexOf(lexeme) + terminalSymbol + 1);
                terminalSymbol += BinaryOperators.Count;
                // Check in unary operators collection
                if (UnaryOperators.Contains(lexeme))
                    return new LexemeResult((TerminalSymbol)UnaryOperators.IndexOf(lexeme) + terminalSymbol + 1);
            }
            return AnalyzeLexemeByAutomata(lexeme);
        }

        private LexemeResult AnalyzeLexemeByAutomata(string lexeme)
        {
            int automataColumn, currentState = 0;
            /* Analyze lexeme parsing each character, using the current state as main reference
             * If the current state turns is about to turn into a null value, it means it has found an unexpected character for the current state,
             * return an error message for said situation
             * If parsing successfully continues until the end, return an error message or terminal symbol according to the last state */
            for (var i = 0; i < lexeme.Length; i++)
            {
                var lexemeCharacter = lexeme[i].ToString();
                if (Numbers.Contains(lexemeCharacter))
                {
                    automataColumn = 0;
                    if (_automata[currentState, automataColumn].Equals(null))
                        return new LexemeResult(LocalizedStrings.LexicalError);
                    else
                        currentState = _automata[currentState, automataColumn].Value;
                }
                else if (DecimalSeparator == lexemeCharacter)
                {
                    automataColumn = 1;
                    if (_automata[currentState, automataColumn].Equals(null))
                        return new LexemeResult(LocalizedStrings.LexicalError);
                    else
                        currentState = _automata[currentState, automataColumn].Value;
                }
                else
                    return new LexemeResult(LocalizedStrings.LexicalError);
            }
            return TerminalSymbolResultByState(currentState);
        }

        private LexemeResult TerminalSymbolResultByState(int lastState)
        {
            switch (lastState)
            {
                // Syntax error if the parser ended up in the following states
                case 0:
                case 1:
                default:
                    return new LexemeResult(LocalizedStrings.LexicalError);
                // Only states 2 and 3 are valid outputs, it means the lexeme is a valid real number
                case 2:
                case 3:
                    return new LexemeResult(TerminalSymbol.RealNumber);
            }
        }

        private LexicalAnalysisResult AddMissingSymbols(LexicalAnalysisResult lexicalAnalysisResult)
        {
            var lexemesList = lexicalAnalysisResult.Lexemes.ToList();
            var terminalSymbolsList = lexicalAnalysisResult.TerminalSymbols.ToList();

            // Insert zero value operand between operation start, and addition or negation operator followed by 
            // operand, opening parenthesis, variable storage character or unary operator
            if (terminalSymbolsList.Count > 1
                && (terminalSymbolsList[0] == TerminalSymbol.AdditionOperator
                    || terminalSymbolsList[0] == TerminalSymbol.SubstractionOperator)
                && (terminalSymbolsList[1] == TerminalSymbol.RealNumber
                    || terminalSymbolsList[1] == TerminalSymbol.OpeningParenthesis
                    || (_terminalSymbolsGroups.TryGetValue(terminalSymbolsList[1], out var tsg)
                        && (tsg == TerminalSymbolGroup.VariableStorageCharacters
                            || tsg == TerminalSymbolGroup.UnaryOperators))))
            {
                lexemesList.Insert(0, ZeroString);
                terminalSymbolsList.Insert(0, TerminalSymbol.RealNumber);
            }

            for (int i = 0; i < terminalSymbolsList.Count - 1; i++)
            {
                // Insert multiplication operator between operand, closing parenthesis or variable storage char, 
                // and opening parenthesis, variable storage character or unary operator
                if ((terminalSymbolsList[i] == TerminalSymbol.RealNumber
                    || terminalSymbolsList[i] == TerminalSymbol.ClosingParenthesis
                    || (_terminalSymbolsGroups.TryGetValue(terminalSymbolsList[i], out var leftTsg)
                        && leftTsg == TerminalSymbolGroup.VariableStorageCharacters))
                    && (terminalSymbolsList[i + 1] == TerminalSymbol.OpeningParenthesis
                    || (_terminalSymbolsGroups.TryGetValue(terminalSymbolsList[i + 1], out var rightTsg)
                        && (rightTsg == TerminalSymbolGroup.VariableStorageCharacters
                            || rightTsg == TerminalSymbolGroup.UnaryOperators))))
                {
                    i++;
                    lexemesList.Insert(i, _operators[TerminalSymbol.MultiplicationOperator].Symbol);
                    terminalSymbolsList.Insert(i, TerminalSymbol.MultiplicationOperator);
                    continue;
                }

                // Introduce prioritary negation operation between operator and negation operator followed by 
                // operand, opening parenthesis, variable storage character or unary operator, 
                // replacing the negation operator with the prioritary negation operation
                if (i < terminalSymbolsList.Count - 2
                    && _terminalSymbolsGroups.TryGetValue(terminalSymbolsList[i], out leftTsg)
                    && (terminalSymbolsList[i] == TerminalSymbol.OpeningParenthesis
                        || leftTsg == TerminalSymbolGroup.BinaryOperators
                        || leftTsg == TerminalSymbolGroup.UnaryOperators)
                    && terminalSymbolsList[i + 1] == TerminalSymbol.SubstractionOperator
                    && (terminalSymbolsList[i + 2] == TerminalSymbol.RealNumber
                        || terminalSymbolsList[i + 2] == TerminalSymbol.OpeningParenthesis
                        || (_terminalSymbolsGroups.TryGetValue(terminalSymbolsList[i + 2], out rightTsg)
                            && (rightTsg == TerminalSymbolGroup.VariableStorageCharacters
                                || rightTsg == TerminalSymbolGroup.UnaryOperators))))
                {
                    i++;
                    lexemesList[i] = _operators[TerminalSymbol.OperandNegatorOperator].Symbol;
                    terminalSymbolsList[i] = TerminalSymbol.OperandNegatorOperator;
                    lexemesList.Insert(i, MinusOneString);
                    terminalSymbolsList.Insert(i, TerminalSymbol.RealNumber);
                    continue;
                }
            }

            return new LexicalAnalysisResult(lexemesList.ToArray(), terminalSymbolsList.ToArray());
        }

        private SyntaxAndSemanticAnalysisResult SyntaxAndSemanticAnalysis(IDictionary<string, decimal> variableStorageValues, LexicalAnalysisResult lexicalAnalysisResult)
        {
            // Data structures and main variables for syntax analysis
            var syntaxQueue = new Queue<TerminalSymbol>(lexicalAnalysisResult.TerminalSymbols);
            syntaxQueue.Enqueue(TerminalSymbol.Nothing);
            var queueElement = syntaxQueue.Dequeue();

            var syntaxStack = new Stack<Enum>();
            syntaxStack.Push(TerminalSymbol.Nothing);
            syntaxStack.Push(_startingNonTerminalSymbol);

            // Data structures for semantic analysis
            var operatorsStack = new Stack<Operator>();
            var postfixOperation = new List<MathObject>();
            while (true)
            {
                var stackElement = syntaxStack.Pop();

                if (stackElement is TerminalSymbol stackElementTs)
                    switch (stackElementTs)
                    {
                        case TerminalSymbol.OpeningParenthesis:
                        case TerminalSymbol.ClosingParenthesis:
                            ProcessMathOperator(operatorsStack, postfixOperation, _operators[queueElement]);
                            break;
                        case TerminalSymbol.RealNumber:
                            postfixOperation.Add(new Operand(decimal.Parse(lexicalAnalysisResult.Lexemes[^syntaxQueue.Count])));
                            break;
                    }
                else if (stackElement is TerminalSymbolGroup stackElementTsg)
                    switch (stackElementTsg)
                    {
                        case TerminalSymbolGroup.VariableStorageCharacters:
                            if (variableStorageValues.TryGetValue(_terminalSymbolsVariableStorageCharacter[queueElement],
                                out var memoryValue))
                                postfixOperation.Add(new Operand(memoryValue));
                            else
                                postfixOperation.Add(new Operand(decimal.Zero));
                            break;
                        case TerminalSymbolGroup.BinaryOperators:
                        case TerminalSymbolGroup.UnaryOperators:
                            ProcessMathOperator(operatorsStack, postfixOperation, _operators[queueElement]);
                            break;
                    }

                if ((_predictiveTableColumns.Contains(queueElement)
                        && queueElement.Equals(stackElement)
                        && !queueElement.Equals(TerminalSymbol.Nothing))
                    || (!_predictiveTableColumns.Contains(queueElement)
                        && _terminalSymbolsGroups[queueElement].Equals(stackElement)
                        && !queueElement.Equals(TerminalSymbol.Nothing)))
                    queueElement = syntaxQueue.Dequeue();
                else if (stackElement is NonTerminalSymbol nonTerminalSymbol)
                {
                    int? nextProductionIndex;
                    if (_predictiveTableColumns.Contains(queueElement))
                        nextProductionIndex = _predictiveTable[nonTerminalSymbol][_predictiveTableColumns.IndexOf(queueElement)];
                    else
                        nextProductionIndex = _predictiveTable[nonTerminalSymbol][_predictiveTableColumns.IndexOf(_terminalSymbolsGroups[queueElement])];

                    if (nextProductionIndex == null)
                        return null;
                    else
                    {
                        // Gets the next production and pushes its elements backwards to the syntax stack
                        var nextProduction = _grammarNonTerminalSymbols[nonTerminalSymbol][nextProductionIndex.Value];
                        foreach (var productionElement in nextProduction.Reverse())
                            syntaxStack.Push(productionElement);
                    }
                }
                else if (queueElement.Equals(stackElement)
                    && queueElement.Equals(TerminalSymbol.Nothing)
                    && !syntaxQueue.Any()
                    && !syntaxStack.Any())
                {
                    EndMathOperatorProcess(operatorsStack, postfixOperation);
                    return new SyntaxAndSemanticAnalysisResult(postfixOperation.ToArray());
                }
                else
                    return null;
            }
        }

        private void ProcessMathOperator(Stack<Operator> operatorsStack, List<MathObject> postfixOperation, Operator actualOperator)
        {
            if (actualOperator.TerminalSymbol == TerminalSymbol.OpeningParenthesis)
                operatorsStack.Push(actualOperator);
            else if (actualOperator.TerminalSymbol == TerminalSymbol.ClosingParenthesis)
            {
                while (operatorsStack.Peek().TerminalSymbol != TerminalSymbol.OpeningParenthesis)
                    postfixOperation.Add(operatorsStack.Pop());
                operatorsStack.Pop();
            }
            else
            {
                while (operatorsStack.Any()
                    && operatorsStack.Peek().Precedence >= actualOperator.Precedence
                    && (!actualOperator.Unary.Value
                        || (actualOperator.Unary.Value
                            && operatorsStack.Peek().Unary.Value))
                    && !operatorsStack.Peek().RightAssociative.Value)
                    postfixOperation.Add(operatorsStack.Pop());
                operatorsStack.Push(actualOperator);
            }
        }

        private void EndMathOperatorProcess(Stack<Operator> operatorsStack, List<MathObject> postfixOperation)
        {
            while (operatorsStack.Any())
                postfixOperation.Add(operatorsStack.Pop());
        }

        private CalculationResult ActualCalculation(SyntaxAndSemanticAnalysisResult syntaxAndSemanticAnalysisResult)
        {
            var postfixOperation = syntaxAndSemanticAnalysisResult.PostfixOperation;
            var operandsStack = new Stack<Operand>();

            try
            {
                foreach (var mathObject in postfixOperation)
                    if (mathObject is Operand operand)
                        operandsStack.Push(operand);
                    else if (mathObject is Operator operatorX)
                        if (operatorX.Unary.Value)
                            operandsStack.Push(EvaluateUnary(operandsStack.Pop(), operatorX));
                        else if (!operatorX.Unary.Value)
                            operandsStack.Push(EvaluateBinary(operandsStack.Pop(), operandsStack.Pop(), operatorX));
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Error: " + exception.Message);
                return new CalculationResult(LocalizedStrings.CalculationError);
            }

            if (operandsStack.Count == 1)
                return new CalculationResult(operandsStack.Pop().Value);
            else
                return new CalculationResult(LocalizedStrings.CalculationError);
        }

        private Operand EvaluateUnary(Operand operand, Operator operatorX) =>
            operatorX.TerminalSymbol switch
            {
                TerminalSymbol.SquareRootOperator => new Operand(DecimalEx.Sqrt(operand.Value)),
                _ => throw new Exception(LocalizedStrings.UnexpectedError),
            };

        private Operand EvaluateBinary(Operand rightOperand, Operand leftOperand, Operator operatorX) =>
            operatorX.TerminalSymbol switch
            {
                TerminalSymbol.AdditionOperator => new Operand(decimal.Add(leftOperand.Value, rightOperand.Value)),
                TerminalSymbol.SubstractionOperator => new Operand(decimal.Subtract(leftOperand.Value, rightOperand.Value)),
                TerminalSymbol.MultiplicationOperator => new Operand(decimal.Multiply(leftOperand.Value, rightOperand.Value)),
                TerminalSymbol.DivisionOperator => new Operand(decimal.Divide(leftOperand.Value, rightOperand.Value)),
                TerminalSymbol.PotentiationOperator => new Operand(DecimalEx.Pow(leftOperand.Value, rightOperand.Value)),
                _ => throw new Exception(LocalizedStrings.UnexpectedError),
            };
    }
}
