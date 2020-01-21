using Calculator.Shared.Models.Enums;

namespace Calculator.Shared.Models.Results
{
    class LexicalAnalysisResult
    {
        public string[] Lexemes { get; }
        public TerminalSymbol[] TerminalSymbols { get; }

        public LexicalAnalysisResult(string[] lexemes, TerminalSymbol[] terminalSymbols)
        {
            Lexemes = lexemes;
            TerminalSymbols = terminalSymbols;
        }
    }
}
