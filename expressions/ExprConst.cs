using static HeatSim.expressions.Managing.ArithmUtils;

namespace HeatSim
{
    public class ExprConst : IExpression
    {
        public static readonly Priority PRIORITY = Priority.NUMBERABLE;

        public readonly BigRational Value;

        public ExprConst(string value)
        {
            Value = new BigRational(value);
        }

        public ExprConst(BigRational value)
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
