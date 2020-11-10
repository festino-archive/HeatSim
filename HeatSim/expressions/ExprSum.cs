using System.Collections.Generic;

namespace HeatSim
{
    public class ExprSum : IExpression
    {
        public static readonly Priority PRIORITY = Priority.ADD;

        private readonly List<IExpression> Args = new List<IExpression>();

        public ExprSum(params IExpression[] args)
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
            if (Args.Count == 0)
                return "0";
            string res = Args[0].AsString();
            for (int i = 1; i < Args.Count; i++)
            {
                string str = Args[i].AsString();
                if (str.StartsWith('-'))
                    res += " " + str;
                else
                    res += " + " + str;
            }
            return res;
            /*string[] strArgs = new string[Args.Count];
            for (int i = 0; i < Args.Count; i++)
            {
                strArgs[i] = Args[i].AsString();
            }
            return string.Join<string>(" + ", strArgs); // TODO sub to avoid brackets*/
        }
    }
}
