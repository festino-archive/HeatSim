using HeatSim.expressions.Managing;
using System.Collections.Generic;
using static HeatSim.expressions.Managing.ArithmUtils;

namespace HeatSim
{
    public static class ExprSimplifier
    {
        public static IExpression Simplify(IExpression init)
        {
            IExpression res = ExprUtils.GetEmptyCopy(init);
            if (res is ExprNaN)
                return res;

            foreach (IExpression _expr in init.GetArgs())
            {
                IExpression expr = _expr;
                expr = Simplify(expr);
                res.AddArg(expr);
            }

            if (res is ExprSum)
                return SimplifySum(res as ExprSum);
            if (res is ExprMult)
                return SimplifyMult(res as ExprMult);
            if (res is ExprDiv)
                return SimplifyMult(new ExprMult(new ExprConst(1), res));
            if (res is ExprPow)
                return SimplifyPow(res as ExprPow);
            if (res is ExprRegFunc)
            {
                ExprRegFunc func = res as ExprRegFunc;
                IExpression[] args = func.GetArgs();
                if (func.Name == "ln" && args[0] is ExprRegFunc && (args[0] as ExprRegFunc).Name == "e")
                    return new ExprConst(1);
            }

                return res;
        }

        private static IExpression SimplifySum(ExprSum init)
        {
            IExpression res = new ExprSum();
            BigRational unitedConst = new BigRational("0");
            foreach (IExpression _expr in init.GetArgs())
            {
                IExpression expr = _expr;
                if (expr is ExprConst)
                {
                    BigRational val = (expr as ExprConst).Value;
                    if (val != 0)
                        unitedConst += val;
                    continue;
                }
                res.AddArg(expr);
            }

            IExpression[] resArgs = res.GetArgs();
            if (resArgs.Length == 0)
                return new ExprConst(unitedConst);
            if (unitedConst != 0)
                res.AddArg(new ExprConst(unitedConst));
            else if (resArgs.Length == 1)
                return resArgs[0];
            return res;
        }

        private static IExpression SimplifyMult(ExprMult init)
        {
            BigRational unitedTopConst = new BigRational("1");
            BigRational unitedBotConst = new BigRational("1");
            IExpression top = new ExprMult();
            IExpression bot = new ExprMult();
            foreach (IExpression _expr in init.GetArgs())
            {
                IExpression res = processMultArg(top, bot, ref unitedTopConst, ref unitedBotConst, _expr);
                if (res != null)
                    return res;
            }

            IExpression[] topArgs = top.GetArgs();
            IExpression[] botArgs = bot.GetArgs();
            if (topArgs.Length == 0)
                top = new ExprConst(unitedTopConst);
            else if (unitedTopConst != 1)
                top.AddArg(new ExprConst(unitedTopConst));
            else if(topArgs.Length == 1)
                top = topArgs[0];
            if (botArgs.Length == 0) {
                if (unitedBotConst == 1)
                    return top;
                else
                    bot = new ExprConst(unitedBotConst);
            }
            else if (unitedBotConst != 1)
                bot.AddArg(new ExprConst(unitedBotConst));
            else if (botArgs.Length == 1)
                bot = botArgs[0];

            // unite (incl neg pow)
            List<IExpression> terms = new List<IExpression>();
            List<IExpression> pows = new List<IExpression>();
            if (!(top is ExprMult))
                AddPowTerm(top, terms, pows, false);
            else
                foreach (IExpression expr in top.GetArgs())
                {
                    AddPowTerm(expr, terms, pows, false);
                }
            if (!(bot is ExprMult))
                AddPowTerm(bot, terms, pows, true);
            else
                foreach (IExpression expr in bot.GetArgs())
                {
                    AddPowTerm(expr, terms, pows, true);
                }
            // tryReduce
            for (int i = 0; i < pows.Count; i++)
                pows[i] = Simplify(pows[i]);
            // update top and bot (only pos pow)
            top = new ExprMult();
            bot = new ExprMult();

            for (int i = 0; i < terms.Count; i++)
            {
                if (pows[i] is ExprConst && (pows[i] as ExprConst).Value < 0)
                    bot.AddArg(new ExprPow(terms[i], new ExprConst(-1 * (pows[i] as ExprConst).Value)));
                else
                    top.AddArg(new ExprPow(terms[i], pows[i]));
            }

            topArgs = top.GetArgs();
            botArgs = bot.GetArgs();
            for (int i = 0; i < topArgs.Length; i++)
                topArgs[i] = Simplify(topArgs[i]);
            for (int i = 0; i < botArgs.Length; i++)
                botArgs[i] = Simplify(botArgs[i]);
            top = new ExprMult(topArgs);
            bot = new ExprMult(botArgs);

            if (topArgs.Length == 0)
                top = new ExprConst(1);
            else if (topArgs.Length == 1)
                top = topArgs[0];
            if (botArgs.Length == 0)
                return top;
            else if (botArgs.Length == 1)
                bot = botArgs[0];

            return new ExprDiv(top, bot);
        }

        private static void AddPowTerm(IExpression term, List<IExpression> terms, List<IExpression> pows, bool reversal)
        {
            IExpression pow = new ExprMult();
            while (term is ExprPow)
            {
                pow.AddArg(term.GetArgs()[1]);
                term = term.GetArgs()[0];
            }
            if (pow.GetArgsCount() == 0)
            {
                if (reversal)
                    pow = new ExprConst(-1);
                else
                    pow = new ExprConst(1);
            }
            else if(reversal)
                pow.AddArg(new ExprConst(-1));

            for (int i = 0; i < terms.Count; i++)
            {
                if (ExprUtils.Equals(terms[i], term))
                {
                    pows[i].AddArg(pow);
                    return;
                }
            }

            terms.Add(term);
            pows.Add(new ExprSum(pow));
        }

        private static IExpression SimplifyPow(ExprPow init)
        {
            IExpression _arg1 = init.GetArgs()[0];
            IExpression _arg2 = init.GetArgs()[1];
            if (_arg2 is ExprConst)
            {
                ExprConst arg2 = _arg2 as ExprConst;
                if (arg2.Value == 0)
                    return new ExprConst(1);
                if (arg2.Value == 1)
                    return init.GetArgs()[0];
            }
            if (_arg1 is ExprConst)
            {
                ExprConst arg1 = _arg1 as ExprConst;
                if (arg1.Value == 0 || arg1.Value == 1)
                    return arg1;
            }
            return init;
        }
        
        private static IExpression processMultArg(IExpression top, IExpression bot, ref BigRational unitedTopConst, ref BigRational unitedBotConst, IExpression arg)
        {
            if (arg is ExprConst)
            {
                BigRational val = (arg as ExprConst).Value;
                if (val == 0)
                    return arg;
                if (val != 1)
                    unitedTopConst *= val;
            }
            else if (arg is ExprDiv)
            {
                IExpression res = processMultArg(top, bot, ref unitedTopConst, ref unitedBotConst, arg.GetArgs()[0]);
                if (res != null)
                    return res;
                res = processMultArg(bot, top, ref unitedBotConst, ref unitedTopConst, arg.GetArgs()[1]);
                if (res != null) // zero div
                    return new ExprNaN();
            }
            else if (arg is ExprMult)
            {
                foreach (IExpression _arg in arg.GetArgs())
                {
                    IExpression res = processMultArg(top, bot, ref unitedTopConst, ref unitedBotConst, _arg);
                    if (res != null)
                        return res;
                }
            }
            else
                top.AddArg(arg);
            return null;
        }

        public static IExpression Substitute(IExpression expr, string varName, IExpression to)
        {
            if (expr is ExprRegFunc && expr.GetArgsCount() == 0 && (expr as ExprRegFunc).Name == varName)
                return to;
            IExpression res = ExprUtils.GetEmptyCopy(expr);
            foreach (IExpression arg in expr.GetArgs())
                res.AddArg(Substitute(arg, varName, to));
            return res;
        }
    }
}
