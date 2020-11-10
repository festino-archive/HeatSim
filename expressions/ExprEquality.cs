using System.Collections.Generic;

namespace HeatSim
{
    class ExprEquality : IExpression
    {
        public static readonly Priority PRIORITY = Priority.OTHER;

        private readonly List<IExpression> Args = new List<IExpression>();

        public ExprEquality(params IExpression[] args)
        {
            Args.AddRange(args);
        }

        public void AddArg(IExpression arg)
        {
            Args.Add(arg);
        }

        public IExpression[] GetArgs()
        {
            return Args.ToArray();
        }

        public int GetArgsCount()
        {
            return Args.Count;
        }

        public IExpression GetOrNaN()
        {
            if (Args.Count == 0)
            {
                return new ExprNaN();
            }
            return this;
        }

        public string AsString()
        {
            string[] strArgs = new string[Args.Count];
            for (int i = 0; i < Args.Count; i++)
            {
                strArgs[i] = Args[i].AsString();
            }
            return string.Join<string>(" = ", strArgs);
        }
    }
}
