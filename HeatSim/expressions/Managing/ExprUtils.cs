namespace HeatSim
{
    public static class ExprUtils
    {
        public static bool Equals(IExpression e1, IExpression e2)
        {
            if (e1.GetType() != e2.GetType() || e1.GetArgsCount() != e2.GetArgsCount())
                return false;
            if (e1 is ExprConst)
                return (e1 as ExprConst).Value == (e2 as ExprConst).Value;
            if (e1 is ExprDouble)
                return (e1 as ExprDouble).Value == (e2 as ExprDouble).Value;
            if (e1 is ExprRegFunc && (e1 as ExprRegFunc).Name != (e2 as ExprRegFunc).Name)
                return false;
            IExpression[] args1 = e1.GetArgs();
            IExpression[] args2 = e2.GetArgs();
            for (int i = 0; i < e1.GetArgsCount(); i++)
                if (!Equals(args1[i], args2[i]))
                    return false;
            return true;
        }

        public static IExpression GetEmptyCopy(IExpression expr)
        {
            if (expr is ExprNaN)
                return expr;
            if (expr is ExprConst || expr is ExprDouble)
                return expr;
            if (expr is ExprSum)
                return new ExprSum();
            if (expr is ExprMult)
                return new ExprMult();
            if (expr is ExprDiv)
                return new ExprDiv();
            if (expr is ExprEquality)
                return new ExprEquality();
            if (expr is ExprPow)
                return new ExprPow();
            if (expr is ExprRegFunc)
                return new ExprRegFunc((expr as ExprRegFunc).Name, (expr as ExprRegFunc).GetArgs().Length);
            if (expr is ExprDerivative)
                return new ExprDerivative((expr as ExprDerivative).Orig, (expr as ExprDerivative).Var);
            return new ExprNaN();
        }

        public static IExpression GetCopy_Slow(IExpression expr)
        {
            return ExprSimplifier.Substitute(expr, "", null);
        }
    }
}
