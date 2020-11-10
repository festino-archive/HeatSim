using System.Collections.Generic;

namespace HeatSim
{
    public class ExprMult : IExpression
    {
        public static readonly Priority PRIORITY = Priority.WEAK_MULT;

        private readonly List<IExpression> Args = new List<IExpression>();

        public ExprMult(params IExpression[] args)
        {
            Args.AddRange(args); // TODO merge Mult and Sum
        }

        public void AddArg(IExpression arg)
        {
            if (arg is ExprConst)
                Args.Insert(0, arg);
            else
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
                return "1";
            if (Args.Count == 1)
                return Args[0].AsString();
            string res = ExprRenderer.AsString(Args[0], PRIORITY);
            for (int i = 1; i < Args.Count; i++)
                res += " * " + ExprRenderer.AsString(Args[i], PRIORITY);
            return res;
            /*string[] strArgs = new string[Args.Count];
            for (int i = 0; i < Args.Count; i++)
            {
                strArgs[i] = Args[i].AsString();
            }
            return "(" + string.Join<string>(") * (", strArgs) + ")"; // TODO compare priority to avoid brackets*/
        }
    }
}
