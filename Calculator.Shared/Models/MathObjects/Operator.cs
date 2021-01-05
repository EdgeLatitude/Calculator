using Calculator.Shared.Models.Enums;

namespace Calculator.Shared.Models.MathObjects
{
    class Operator : MathObject
    {
        public TerminalSymbol TerminalSymbol { get; }
        public int Precedence { get; }
        public string Symbol { get; }
        public bool? RightAssociative { get; }
        public bool? Unary { get; }

        public Operator(TerminalSymbol terminalSymbol, int precedence, string symbol)
        {
            TerminalSymbol = terminalSymbol;
            Precedence = precedence;
            Symbol = symbol;
        }

        public Operator(TerminalSymbol terminalSymbol, int precedence, string symbol, bool rightAssociative, bool unary)
        {
            TerminalSymbol = terminalSymbol;
            Precedence = precedence;
            Symbol = symbol;
            RightAssociative = rightAssociative;
            Unary = unary;
        }
    }
}
