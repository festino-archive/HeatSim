using System;

namespace HeatSim
{
    class ExprDouble : IExpression
    {
        public static readonly Priority PRIORITY = Priority.NUMBERABLE;

        public readonly double Value;

        public ExprDouble(string value)
        {
            try
            {
                Value = double.Parse(value);
            }
            catch (FormatException)
            {
                Value = 0;
            }
        }

        public ExprDouble(double value)
        {
            Value = value;
        }

        public void AddArg(IExpression arg) { }

        public IExpression[] GetArgs()
        {
            return new IExpression[0];
        }

        public int GetArgsCount()
        {
            return 0;
        }

        public IExpression GetOrNaN()
        {
            return this;
        }

        public string AsString()
        {
            return Value.ToString();
        }
    }
}
