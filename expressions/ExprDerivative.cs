namespace HeatSim
{
    class ExprDerivative : IExpression
    {
        public static readonly Priority PRIORITY = Priority.NUMBERABLE;

        public readonly ExprRegFunc Orig;
        public readonly string Var;

        public ExprDerivative(ExprRegFunc func, string varName)
        {
            Orig = func;
            Var = varName;
        }

        public void AddArg(IExpression arg) { }

        public IExpression[] GetArgs()
        {
            return new IExpression[] { Orig };
        }

        public int GetArgsCount()
        {
            return 1;
        }

        public IExpression GetOrNaN()
        {
            return this;
        }

        public string AsString()
        {
            return "(" + Orig.AsString() + ")_" + Var;
        }
    }
}
