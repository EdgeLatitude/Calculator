namespace Calculator.Shared.Models.Results
{
    class CalculationResult
    {
        public bool Successful { get; }
        public string ErrorMessage { get; }
        public double Result { get; }

        public CalculationResult(double result)
        {
            Successful = true;
            Result = result;
        }

        public CalculationResult(string errorMessage)
        {
            Successful = false;
            ErrorMessage = errorMessage;
        }
    }
}
