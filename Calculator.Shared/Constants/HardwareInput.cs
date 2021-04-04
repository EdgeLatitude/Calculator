namespace Calculator.Shared.Constants
{
    public static class HardwareInput
    {
        public const string CopyCharacter = "c";
        public const string PasteCharacter = "v";
        public const string RootCharacter = "r";
        public const string ResultOperator = LexicalSymbols.ResultOperator;

        public static readonly string[] ParenthesesDecimalSeparatorsAndOperators = new string[]
        {
            LexicalSymbols.OpeningParenthesis,
            LexicalSymbols.ClosingParenthesis,
            LexicalSymbols.Comma,
            LexicalSymbols.Dot,
            LexicalSymbols.AdditionOperator,
            LexicalSymbols.SubstractionOperator,
            LexicalSymbols.MultiplicationOperator,
            LexicalSymbols.DivisionOperator,
            LexicalSymbols.PotentiationOperator,
            LexicalSymbols.SquareRootOperator,
            LexicalSymbols.SimpleMultiplicationOperator,
            LexicalSymbols.SimpleDivisionOperator
        };
    }
}
