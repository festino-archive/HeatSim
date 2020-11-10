namespace HeatSim
{
    public class ExprRegFunc : IExpression
    {
        public static readonly Priority PRIORITY = Priority.NUMBERABLE;

        public readonly string Name;
        private readonly IExpression[] Args;
        private int CurCount = 0;
        public int ArgCount { get => Args.Length; }

        public ExprRegFunc(string name, int argCount, params IExpression[] args)
        {
            Name = name;
            Args = new IExpression[argCount];
            for (; CurCount < argCount && CurCount < args.Length; CurCount++)
                Args[CurCount] = args[CurCount];
        }

        public void AddArg(IExpression arg)
        {
            if (CurCount < ArgCount)
            {
                Args[CurCount] = arg;
                CurCount++;
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
            if (CurCount < ArgCount)
            {
                return new ExprNaN();
            }
            return this;
        }

        public string AsString()
        {
            if (CurCount < ArgCount)
            {
                return $"{Name}{{{CurCount}/{ArgCount}}}";
            }
            if (ArgCount == 0)
                return Name;

            string[] strArgs = new string[ArgCount];
            for (int i = 0; i < ArgCount; i++)
            {
                strArgs[i] = Args[i].AsString();
            }
            return Name + "(" + string.Join<string>(", ", strArgs) + ")";
        }
    }
}
