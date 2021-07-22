namespace Calculator.Shared.Models.MathObjects
{
    internal class Operand : MathObject
    {
        public decimal Value { get; }

        public Operand(decimal value)
        {
            Value = value;
        }
    }
}
