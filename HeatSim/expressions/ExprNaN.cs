namespace HeatSim
{
    public class ExprNaN : IExpression
    {
        public static readonly Priority PRIORITY = Priority.OTHER;
        public void AddArg(IExpression arg) {}

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
            return "NaN";
        }
    }
}
