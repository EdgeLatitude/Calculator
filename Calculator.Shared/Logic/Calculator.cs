﻿using Calculator.Shared.Localization;
using Calculator.Shared.Models.Enums;
using Calculator.Shared.Models.MathObjects;
using Calculator.Shared.Models.Results;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Calculator.Shared.Logic
{
    static class Calculator
    {
        // Compilation constants
        public const string WhiteSpace = " ";
        public const double Zero = 0;
        public const string ZeroString = "0";
        public const string NegativeOneString = "-1";

        // Runtime constants
        public static readonly char LastResult
            = LocalizedStrings.LastResultCharacter[0];
        public static readonly string DecimalSeparator
            = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        #region General definitions
        private static readonly Dictionary<TerminalSymbol, char> TerminalSymbolsVariableStorageCharacter
            = new Dictionary<TerminalSymbol, char>
        {
            { TerminalSymbol.LastResult, LastResult }
        };
        #endregion

        #region Lexical definitions
        // Lexical collections
        public static readonly char[] VariableStorageCharacters
            = new char[] { LastResult }; // 1 terminal symbol for each
        private static readonly char[] Parentheses
            = new char[] { '(', ')' }; // 1 terminal symbol for each
        private static readonly char[] BinaryOperators
            = new char[] { '+', '-', '×', '÷', '^' }; // 1 terminal symbol for each
        private static readonly char[] UnaryOperators
            = new char[] { '√' }; // 1 terminal symbol for each
        private static readonly char[] Numbers
            = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }; // 1 terminal symbol for a complete real number

        // Separators array to be filled with some lexical collections
        private static readonly char[] Separators;

        // Automata for lexical analysis
        private static readonly int?[,] Automata = new int?[,]
        {           //  Numbers     Decimal
        /* S0 */    {   2,          1,      }, // Initial state
        /* S1 */    {   3,          null,   },
        /* S2 */    {   2,          1,      }, // Accepted state ✓
        /* S3 */    {   3,          null,   }  // Accepted state ✓
        };
        #endregion

        #region Syntax definitions
        // Terminal symbols groups
        private static readonly Dictionary<TerminalSymbol, TerminalSymbolGroup> TerminalSymbolsGroups
            = new Dictionary<TerminalSymbol, TerminalSymbolGroup>
        {
            { TerminalSymbol.None, TerminalSymbolGroup.None },
            { TerminalSymbol.LastResult, TerminalSymbolGroup.VariableStorageCharacters },
            { TerminalSymbol.OpeningParentheses, TerminalSymbolGroup.Parentheses },
            { TerminalSymbol.ClosingParentheses, TerminalSymbolGroup.Parentheses },
            { TerminalSymbol.AdditionOperator, TerminalSymbolGroup.BinaryOperators },
            { TerminalSymbol.SubstractionOperator, TerminalSymbolGroup.BinaryOperators },
            { TerminalSymbol.MultiplicationOperator, TerminalSymbolGroup.BinaryOperators },
            { TerminalSymbol.DivisionOperator, TerminalSymbolGroup.BinaryOperators },
            { TerminalSymbol.PotentiationOperator, TerminalSymbolGroup.BinaryOperators },
            { TerminalSymbol.SquareRootOperator, TerminalSymbolGroup.UnaryOperators },
            { TerminalSymbol.RealNumber, TerminalSymbolGroup.None },
            { TerminalSymbol.Nothing, TerminalSymbolGroup.None },
            { TerminalSymbol.OperandNegatorOperator, TerminalSymbolGroup.BinaryOperators }
        };

        // Grammar productions for each non terminal symbol
        private static readonly Dictionary<NonTerminalSymbol, Enum[][]> GrammarNonTerminalSymbols
            = new Dictionary<NonTerminalSymbol, Enum[][]>
        {
            { NonTerminalSymbol.Expression, new Enum[][]
                {
                    new Enum[] { NonTerminalSymbol.Operand, NonTerminalSymbol.BinaryOperator },
                    new Enum[] { TerminalSymbolGroup.UnaryOperators, NonTerminalSymbol.Expression },
                    new Enum[] { TerminalSymbol.OpeningParentheses, NonTerminalSymbol.Expression, TerminalSymbol.ClosingParentheses, NonTerminalSymbol.BinaryOperator }
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
        };

        // Predictive table for each non terminal symbol
        private static readonly Dictionary<NonTerminalSymbol, int?[]> PredictiveTable
            = new Dictionary<NonTerminalSymbol, int?[]>
        {
            { NonTerminalSymbol.Expression,
                new int?[] { 0000, 0002, null, null, 0001, 0000, null } },
            { NonTerminalSymbol.Operand,
                new int?[] { 0001, null, null, null, null, 0000, null } },
            { NonTerminalSymbol.MemoryOption,
                new int?[] { 0000, null, 0001, 0001, null, null, 0001 } },
            { NonTerminalSymbol.BinaryOperator,
                new int?[] { null, null, 0001, 0000, null, null, 0001 } }
        };

        // Predictive table column definitions
        private static readonly Enum[] PredictiveTableColumns = new Enum[]
        {
            TerminalSymbolGroup.VariableStorageCharacters,
            TerminalSymbol.OpeningParentheses,
            TerminalSymbol.ClosingParentheses,
            TerminalSymbolGroup.BinaryOperators,
            TerminalSymbolGroup.UnaryOperators,
            TerminalSymbol.RealNumber,
            TerminalSymbol.Nothing
        };

        // Starting non terminal symbol
        private const NonTerminalSymbol StartingNonTerminalSymbol = NonTerminalSymbol.Expression;
        #endregion

        #region Semantic definitions
        // Operators dictionary by token
        private static readonly Dictionary<TerminalSymbol, Operator> Operators
            = new Dictionary<TerminalSymbol, Operator>
        {
            { TerminalSymbol.OpeningParentheses, new Operator(TerminalSymbol.OpeningParentheses, 0, '(') },
            { TerminalSymbol.ClosingParentheses, new Operator(TerminalSymbol.ClosingParentheses, 0, ')') },
            { TerminalSymbol.AdditionOperator, new Operator(TerminalSymbol.AdditionOperator, 1, '+', false, false) },
            { TerminalSymbol.SubstractionOperator, new Operator(TerminalSymbol.SubstractionOperator, 1, '-', false, false) },
            { TerminalSymbol.MultiplicationOperator, new Operator(TerminalSymbol.MultiplicationOperator, 2, '×', false, false) },
            { TerminalSymbol.DivisionOperator, new Operator(TerminalSymbol.DivisionOperator, 2, '÷', false, false) },
            { TerminalSymbol.PotentiationOperator, new Operator(TerminalSymbol.PotentiationOperator, 3, '^', false, false) },
            { TerminalSymbol.SquareRootOperator, new Operator(TerminalSymbol.SquareRootOperator, 3, '√', false, true) },
            { TerminalSymbol.OperandNegatorOperator, new Operator(TerminalSymbol.MultiplicationOperator, int.MaxValue, '×', false, false) }
        };
        #endregion

        static Calculator()
        {
            // Initialize separators array from list filled with specified lexical collections
            var separators = new List<char>();
            separators.AddRange(VariableStorageCharacters);
            separators.AddRange(Parentheses);
            separators.AddRange(BinaryOperators);
            separators.AddRange(UnaryOperators);
            Separators = separators.ToArray();
        }

        public static CalculationResult Calculate(string operation, Dictionary<char, double> variableStorageValues)
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

        private static string[] SplitOperation(string operation)
        {
            // Add surrounding blank spaces to every separator in the operation
            foreach (var separator in Separators)
                if (operation.Contains(separator))
                    operation = operation.Replace(separator.ToString(), WhiteSpace + separator + WhiteSpace);
            // Remove unnecessary blank spaces
            var doubleWhiteSpace = WhiteSpace + WhiteSpace;
            while (operation.Contains(doubleWhiteSpace))
                operation = operation.Replace(doubleWhiteSpace, WhiteSpace);
            operation = operation.Trim();
            // Split into the lexemes array using blank spaces
            return operation.Split();
        }

        private static LexicalAnalysisResult LexicalAnalysis(string[] lexemes)
        {
            var terminalSymbols = new List<TerminalSymbol>();
            // Use the lexemes array to fill corresponding terminal symbols list
            foreach (var lexeme in lexemes)
            {
                var lexemeResult = AnalyzeLexeme(lexeme);
                if (lexemeResult != null)
                    if (lexemeResult.Successful)
                        terminalSymbols.Add(lexemeResult.TerminalSymbol);
                    else
                        return null;
                else
                    return null;
            }
            return new LexicalAnalysisResult(lexemes, terminalSymbols.ToArray());
        }

        private static LexemeResult AnalyzeLexeme(string lexeme)
        {
            var terminalSymbol = 0;
            // Check for lexeme in each lexical collection, at the end parse accumulated terminal symbol index into an actual symbol
            // Check in variable storage characters collection
            if (lexeme.Length == 1)
            {
                var charLexeme = char.Parse(lexeme);
                if (VariableStorageCharacters.Contains(charLexeme))
                    return new LexemeResult((TerminalSymbol)Array.IndexOf(VariableStorageCharacters, charLexeme) + terminalSymbol + 1);
            }
            terminalSymbol += VariableStorageCharacters.Length;
            // Check in special characters and operators collections
            if (lexeme.Length == 1)
            {
                var lexemeAsCharacter = lexeme.ToCharArray()[0];
                // Check in special characters collection
                if (Parentheses.Contains(lexemeAsCharacter))
                    return new LexemeResult((TerminalSymbol)Array.IndexOf(Parentheses, lexemeAsCharacter) + terminalSymbol + 1);
                terminalSymbol += Parentheses.Length;
                // Check in binary operators collection
                if (BinaryOperators.Contains(lexemeAsCharacter))
                    return new LexemeResult((TerminalSymbol)Array.IndexOf(BinaryOperators, lexemeAsCharacter) + terminalSymbol + 1);
                terminalSymbol += BinaryOperators.Length;
                // Check in unary operators collection
                if (UnaryOperators.Contains(lexemeAsCharacter))
                    return new LexemeResult((TerminalSymbol)Array.IndexOf(UnaryOperators, lexemeAsCharacter) + terminalSymbol + 1);
            }
            return AnalyzeLexemeByAutomata(lexeme);
        }

        private static LexemeResult AnalyzeLexemeByAutomata(string lexeme)
        {
            char lexemeCharacter;
            int automataColumn, currentState = 0;
            /* Analyze lexeme parsing each character, using the current state as main reference
             * If the current state turns is about to turn into a null value, it means it has found an unexpected character for the current state,
             * return an error message for said situation
             * If parsing successfully continues until the end, return an error message or terminal symbol according to the last state */
            for (var i = 0; i < lexeme.Length; i++)
            {
                lexemeCharacter = lexeme[i];
                if (Numbers.Contains(lexemeCharacter))
                {
                    automataColumn = 0;
                    if (Automata[currentState, automataColumn].Equals(null))
                        return new LexemeResult(LocalizedStrings.LexicalError);
                    else
                        currentState = Automata[currentState, automataColumn].Value;
                }
                else if (DecimalSeparator == lexemeCharacter.ToString())
                {
                    automataColumn = 1;
                    if (Automata[currentState, automataColumn].Equals(null))
                        return new LexemeResult(LocalizedStrings.LexicalError);
                    else
                        currentState = Automata[currentState, automataColumn].Value;
                }
                else
                    return new LexemeResult(LocalizedStrings.LexicalError);
            }
            return TerminalSymbolResultByState(currentState);
        }

        private static LexemeResult TerminalSymbolResultByState(int lastState)
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

        private static LexicalAnalysisResult AddMissingSymbols(LexicalAnalysisResult lexicalAnalysisResult)
        {
            var lexemesList = lexicalAnalysisResult.Lexemes.ToList();
            var terminalSymbolsList = lexicalAnalysisResult.TerminalSymbols.ToList();

            // Insert zero value operand between operation start, and addition or negation operator followed by 
            // operand, opening parentheses, variable storage character or unary operator
            if (terminalSymbolsList.Count > 1
                && (terminalSymbolsList[0] == TerminalSymbol.AdditionOperator
                    || terminalSymbolsList[0] == TerminalSymbol.SubstractionOperator)
                && (terminalSymbolsList[1] == TerminalSymbol.RealNumber
                    || terminalSymbolsList[1] == TerminalSymbol.OpeningParentheses
                    || (TerminalSymbolsGroups.TryGetValue(terminalSymbolsList[1], out var tsg)
                        && (tsg == TerminalSymbolGroup.VariableStorageCharacters
                            || tsg == TerminalSymbolGroup.UnaryOperators))))
            {
                lexemesList.Insert(0, ZeroString);
                terminalSymbolsList.Insert(0, TerminalSymbol.RealNumber);
            }

            for (int i = 0; i < terminalSymbolsList.Count - 1; i++)
            {
                // Insert multiplication operator between operand, closing parentheses or variable storage char, 
                // and opening parentheses, variable storage character or unary operator
                if ((terminalSymbolsList[i] == TerminalSymbol.RealNumber
                    || terminalSymbolsList[i] == TerminalSymbol.ClosingParentheses
                    || (TerminalSymbolsGroups.TryGetValue(terminalSymbolsList[i], out var leftTsg)
                        && leftTsg == TerminalSymbolGroup.VariableStorageCharacters))
                    && (terminalSymbolsList[i + 1] == TerminalSymbol.OpeningParentheses
                    || (TerminalSymbolsGroups.TryGetValue(terminalSymbolsList[i + 1], out var rightTsg)
                        && (rightTsg == TerminalSymbolGroup.VariableStorageCharacters
                            || rightTsg == TerminalSymbolGroup.UnaryOperators))))
                {
                    i++;
                    lexemesList.Insert(i, Operators[TerminalSymbol.MultiplicationOperator].Symbol);
                    terminalSymbolsList.Insert(i, TerminalSymbol.MultiplicationOperator);
                    continue;
                }

                // Introduce prioritary negation operation between operator and negation operator followed by 
                // operand, opening parentheses, variable storage character or unary operator, 
                // replacing the negation operator with the prioritary negation operation
                if (i < terminalSymbolsList.Count - 2
                    && TerminalSymbolsGroups.TryGetValue(terminalSymbolsList[i], out leftTsg)
                    && (terminalSymbolsList[i] == TerminalSymbol.OpeningParentheses
                        || leftTsg == TerminalSymbolGroup.BinaryOperators
                        || leftTsg == TerminalSymbolGroup.UnaryOperators)
                    && terminalSymbolsList[i + 1] == TerminalSymbol.SubstractionOperator
                    && (terminalSymbolsList[i + 2] == TerminalSymbol.RealNumber
                        || terminalSymbolsList[i + 2] == TerminalSymbol.OpeningParentheses
                        || (TerminalSymbolsGroups.TryGetValue(terminalSymbolsList[i + 2], out rightTsg)
                            && (rightTsg == TerminalSymbolGroup.VariableStorageCharacters
                                || rightTsg == TerminalSymbolGroup.UnaryOperators))))
                {
                    i++;
                    lexemesList[i] = Operators[TerminalSymbol.OperandNegatorOperator].Symbol;
                    terminalSymbolsList[i] = TerminalSymbol.OperandNegatorOperator;
                    lexemesList.Insert(i, NegativeOneString);
                    terminalSymbolsList.Insert(i, TerminalSymbol.RealNumber);
                    continue;
                }
            }

            return new LexicalAnalysisResult(lexemesList.ToArray(), terminalSymbolsList.ToArray());
        }

        private static SyntaxAndSemanticAnalysisResult SyntaxAndSemanticAnalysis(Dictionary<char, double> variableStorageValues, LexicalAnalysisResult lexicalAnalysisResult)
        {
            // Data structures and main variables for syntax analysis
            var syntaxQueue = new Queue<TerminalSymbol>(lexicalAnalysisResult.TerminalSymbols);
            syntaxQueue.Enqueue(TerminalSymbol.Nothing);
            var queueElement = syntaxQueue.Dequeue();

            var syntaxStack = new Stack<Enum>();
            syntaxStack.Push(TerminalSymbol.Nothing);
            syntaxStack.Push(StartingNonTerminalSymbol);

            // Data structures for semantic analysis
            var operatorsStack = new Stack<Operator>();
            var postfixOperation = new List<MathObject>();
            while (true)
            {
                var stackElement = syntaxStack.Pop();

                if (stackElement is TerminalSymbol stackElementTs)
                    switch (stackElementTs)
                    {
                        case TerminalSymbol.OpeningParentheses:
                        case TerminalSymbol.ClosingParentheses:
                            ProcessMathOperator(operatorsStack, postfixOperation, Operators[queueElement]);
                            break;
                        case TerminalSymbol.RealNumber:
                            postfixOperation.Add(new Operand(double.Parse(lexicalAnalysisResult.Lexemes[lexicalAnalysisResult.Lexemes.Length - syntaxQueue.Count])));
                            break;
                    }
                else if (stackElement is TerminalSymbolGroup stackElementTsg)
                    switch (stackElementTsg)
                    {
                        case TerminalSymbolGroup.VariableStorageCharacters:
                            if (variableStorageValues.TryGetValue(TerminalSymbolsVariableStorageCharacter[queueElement],
                                out var memoryValue))
                                postfixOperation.Add(new Operand(memoryValue));
                            else
                                postfixOperation.Add(new Operand(Zero));
                            break;
                        case TerminalSymbolGroup.BinaryOperators:
                        case TerminalSymbolGroup.UnaryOperators:
                            ProcessMathOperator(operatorsStack, postfixOperation, Operators[queueElement]);
                            break;
                    }

                if ((PredictiveTableColumns.Contains(queueElement)
                        && queueElement.Equals(stackElement)
                        && !queueElement.Equals(TerminalSymbol.Nothing))
                    || (!PredictiveTableColumns.Contains(queueElement)
                        && TerminalSymbolsGroups[queueElement].Equals(stackElement)
                        && !queueElement.Equals(TerminalSymbol.Nothing)))
                    queueElement = syntaxQueue.Dequeue();
                else if (stackElement is NonTerminalSymbol nonTerminalSymbol)
                {
                    int? nextProductionIndex;
                    if (PredictiveTableColumns.Contains(queueElement))
                        nextProductionIndex = PredictiveTable[nonTerminalSymbol][Array.IndexOf(PredictiveTableColumns, queueElement)];
                    else
                        nextProductionIndex = PredictiveTable[nonTerminalSymbol][Array.IndexOf(PredictiveTableColumns, TerminalSymbolsGroups[queueElement])];

                    if (nextProductionIndex == null)
                        return null;
                    else
                    {
                        // Gets the next production and pushes its elements backwards to the syntax stack
                        var nextProduction = GrammarNonTerminalSymbols[nonTerminalSymbol][nextProductionIndex.Value];
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

        private static void ProcessMathOperator(Stack<Operator> operatorsStack, List<MathObject> postfixOperation, Operator actualOperator)
        {
            if (actualOperator.TerminalSymbol == TerminalSymbol.OpeningParentheses)
                operatorsStack.Push(actualOperator);
            else if (actualOperator.TerminalSymbol == TerminalSymbol.ClosingParentheses)
            {
                while (operatorsStack.Peek().TerminalSymbol != TerminalSymbol.OpeningParentheses)
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

        private static void EndMathOperatorProcess(Stack<Operator> operatorsStack, List<MathObject> postfixOperation)
        {
            while (operatorsStack.Any())
                postfixOperation.Add(operatorsStack.Pop());
        }

        private static CalculationResult ActualCalculation(SyntaxAndSemanticAnalysisResult syntaxAndSemanticAnalysisResult)
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

        private static Operand EvaluateUnary(Operand operand, Operator operatorX)
        {
            switch (operatorX.TerminalSymbol)
            {
                case TerminalSymbol.SquareRootOperator:
                    return new Operand(Math.Sqrt(operand.Value));
                default:
                    throw new Exception(LocalizedStrings.UnexpectedError);
            }
        }

        private static Operand EvaluateBinary(Operand rightOperand, Operand leftOperand, Operator operatorX)
        {
            switch (operatorX.TerminalSymbol)
            {
                case TerminalSymbol.AdditionOperator:
                    return new Operand(leftOperand.Value + rightOperand.Value);
                case TerminalSymbol.SubstractionOperator:
                    return new Operand(leftOperand.Value - rightOperand.Value);
                case TerminalSymbol.MultiplicationOperator:
                    return new Operand(leftOperand.Value * rightOperand.Value);
                case TerminalSymbol.DivisionOperator:
                    return new Operand(leftOperand.Value / rightOperand.Value);
                case TerminalSymbol.PotentiationOperator:
                    return new Operand(Math.Pow(leftOperand.Value, rightOperand.Value));
                default:
                    throw new Exception(LocalizedStrings.UnexpectedError);
            }
        }
    }
}