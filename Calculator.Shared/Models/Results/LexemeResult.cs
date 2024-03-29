﻿using Calculator.Shared.Models.Enums;

namespace Calculator.Shared.Models.Results
{
    internal class LexemeResult
    {
        public bool Successful { get; }
        public string ErrorMessage { get; }
        public TerminalSymbol TerminalSymbol { get; }

        public LexemeResult(TerminalSymbol terminalSymbol)
        {
            Successful = true;
            TerminalSymbol = terminalSymbol;
        }

        public LexemeResult(string errorMessage)
        {
            Successful = false;
            ErrorMessage = errorMessage;
        }
    }
}
