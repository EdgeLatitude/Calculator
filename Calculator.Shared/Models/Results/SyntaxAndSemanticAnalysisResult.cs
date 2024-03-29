﻿using Calculator.Shared.Models.MathObjects;

namespace Calculator.Shared.Models.Results
{
    internal class SyntaxAndSemanticAnalysisResult
    {
        public MathObject[] PostfixOperation { get; }

        public SyntaxAndSemanticAnalysisResult(MathObject[] postfixOperation)
        {
            PostfixOperation = postfixOperation;
        }
    }
}
