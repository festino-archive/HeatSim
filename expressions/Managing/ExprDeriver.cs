namespace HeatSim
{
    public static class ExprDeriver
    {
        public static IExpression Derive(IExpression init, ExprConst var)
        {
            return Derive(init, var.AsString());
        }
        public static IExpression Derive(IExpression init, string varName)
        {
            if (init is ExprNaN)
                return init;
            if (init is ExprConst)
            {
                return new ExprConst("0");
            }
            if (init is ExprSum)
            {
                ExprSum res = new ExprSum();
                foreach (IExpression deriving in init.GetArgs())
                    res.AddArg(Derive(deriving, varName));
                return res;
            }
            if (init is ExprEquality)
            {
                ExprEquality res = new ExprEquality();
                foreach (IExpression deriving in init.GetArgs())
                    res.AddArg(Derive(deriving, varName));
                return res;
            }
            if (init is ExprMult)
            {
                ExprSum res = new ExprSum();
                IExpression[] args = init.GetArgs();
                for (int i = 0; i < args.Length; i++)
                {
                    IExpression deriving = args[i];
                    ExprMult expr = new ExprMult();
                    expr.AddArg(Derive(deriving, varName));
                    for (int j = 0; j < args.Length; j++)
                        if (i != j)
                            expr.AddArg(args[j]);
                    res.AddArg(expr);
                }
                return res;
            }
            if (init is ExprDiv) // (u/v)' = (u'v - v'u)/v^2
            {
                IExpression[] args = init.GetArgs();
                IExpression first = new ExprMult(Derive(args[0], varName), args[1]);
                IExpression second = new ExprMult(Derive(args[1], varName), args[0]);
                IExpression top = new ExprSum(first, new ExprMult(new ExprConst("-1"), second));
                return new ExprDiv(top, new ExprPow(args[1], new ExprConst("2")));
            }
            if (init is ExprPow) // (a^b)' = (e^(b ln a))' = e^(b ln a)*(b ln a)' = a^b*(b'ln a + a'b/a)
            {
                IExpression[] args = init.GetArgs();
                ExprRegFunc ln_a = new ExprRegFunc("ln", 1, args[0]);
                IExpression first = new ExprMult(Derive(args[1], varName), ln_a);
                IExpression second = new ExprDiv(new ExprMult(Derive(args[0], varName), args[1]), args[0]);
                return new ExprMult(init, new ExprSum(first, second));
            }
            init = init.GetOrNaN();
            if (init is ExprRegFunc)
            {
                ExprRegFunc func = init as ExprRegFunc;
                IExpression[] args = func.GetArgs();
                // TODO function wrappers (alias + argcount + etc)
                if (args.Length == 0)
                {
                    if (func.Name == varName)
                        return new ExprConst("1");
                    else
                        return new ExprConst("0");
                }
                if (args.Length == 1)
                {
                    IExpression first = args[0];
                    IExpression firstD = Derive(first, varName);
                    if (func.Name == "abs")
                        return new ExprMult(firstD, new ExprRegFunc("sgn", 1, first));
                    if (func.Name == "ln")
                        return new ExprDiv(firstD, first);
                    if (func.Name == "sin") // (sin f)' = f'cos f
                        return new ExprMult(firstD, new ExprRegFunc("cos", 1, first));
                    if (func.Name == "cos") // (cos f)' = -f'sin f
                        return new ExprMult(firstD, new ExprConst("-1"), new ExprRegFunc("sin", 1, first));
                    if (func.Name == "tg") // (tg f)' = f'/cos^2 f
                        return new ExprDiv(firstD, new ExprPow(new ExprRegFunc("cos", 1, first), new ExprConst("2")));
                    if (func.Name == "ctg") // (ctg f)' = f'/(-sin^2 f)
                        return new ExprMult(firstD, new ExprDiv(new ExprConst("-1"), new ExprPow(new ExprRegFunc("sin", 1, first), new ExprConst("2"))));
                    if (func.Name == "arcsin")
                    {
                        IExpression inSqrt = new ExprSum(new ExprConst("1"), new ExprMult(new ExprConst("-1"), new ExprPow(first, new ExprConst("2"))));
                        return new ExprDiv(firstD, new ExprPow(inSqrt, new ExprConst("1/2")));
                    }
                    if (func.Name == "arccos")
                    {
                        IExpression inSqrt = new ExprSum(new ExprConst("1"), new ExprMult(new ExprConst("-1"), new ExprPow(first, new ExprConst("2"))));
                        return new ExprMult(firstD, new ExprDiv(new ExprConst("-1"), new ExprPow(inSqrt, new ExprConst("1/2"))));
                    }
                    if (func.Name == "arctg") // (arctg f)' = f'/(1 + f^2)
                        return new ExprDiv(firstD, new ExprSum(new ExprConst("1"), new ExprPow(first, new ExprConst("2"))));
                    if (func.Name == "arcctg") // (arctg f)' = -f'/(1 + f^2)
                        return new ExprMult(firstD, new ExprDiv(new ExprConst("-1"), new ExprSum(new ExprConst("1"), new ExprPow(first, new ExprConst("2")))));
                    if (func.Name == "sh") // (sh f)' = f'ch f
                        return new ExprMult(firstD, new ExprRegFunc("ch", 1, first));
                    if (func.Name == "ch") // (ch f)' = f'sh f
                        return new ExprMult(firstD, new ExprRegFunc("sh", 1, first));
                }
                return new ExprDerivative(func, varName);
            }
            return new ExprNaN();
        }
    }
}
