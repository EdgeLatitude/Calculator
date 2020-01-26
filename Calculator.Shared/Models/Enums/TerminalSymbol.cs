﻿namespace Calculator.Shared.Models.Enums
{
    enum TerminalSymbol
    {
        None, // Used when no valid lexeme is detected
        LastResult, // Variable storage characters
        OpeningParentheses, ClosingParentheses, // Special characters
        AdditionOperator, SubstractionOperator, MultiplicationOperator, DivisionOperator, PotentiationOperator, // Binary operators
        SquareRootOperator, // Unary operators
        RealNumber, // Numbers (any)
        Nothing,
        OperandNegatorOperator // Used for implicit operand negation
    }
}