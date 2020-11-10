namespace HeatSim
{
    public class ExprDiv : IExpression
    {
        public static readonly Priority PRIORITY = Priority.DIV;

        private int ArgCount = 0;
        private readonly IExpression[] Args = new IExpression[2];

        public ExprDiv() { }

        public ExprDiv(IExpression arg1, IExpression arg2)
        {
            Args[0] = arg1;
            Args[1] = arg2;
            ArgCount = 2;
        }

        public void AddArg(IExpression arg)
        {
            if (ArgCount < 2)
            {
                Args[ArgCount] = arg;
                ArgCount++;
            }
        }

        public IExpression[] GetArgs()
        {
            return Args; // TODO deep copy
        }

        public int GetArgsCount()
        {
            return Args.Length;
        }

        public IExpression GetOrNaN()
        {
            if (ArgCount < 2)
            {
                return new ExprNaN();
            }
            return this;
        }

        public string AsString()
        {
            if (ArgCount < 2)
            {
                return $"DIV{{{ArgCount}/2}}";
            }
            return ExprRenderer.AsString(Args[0], PRIORITY + 1) + " / " + ExprRenderer.AsString(Args[1], PRIORITY);
        }
    }
}
