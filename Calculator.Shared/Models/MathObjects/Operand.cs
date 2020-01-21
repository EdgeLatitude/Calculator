namespace Calculator.Shared.Models.MathObjects
{
    class Operand : MathObject
    {
        public double Value { get; }

        public Operand(double value)
        {
            Value = value;
        }
    }
}
