using System;
using System.Collections.Generic;

namespace HeatSim
{
    // COPY OF ExprSimplifier WITH ExprConst->ExprDouble, BigRational->double
    public class ExprDoubleSimplifier
    {
        public static IExpression SimplifyDouble(IExpression init)
        {
            if (init.GetArgsCount() == 0)
            {
                if (init is ExprDouble)
                    return init;
                if (init is ExprRegFunc)
                {
                    string name = (init as ExprRegFunc).Name;
                    if (name == MathAliases.ConvertName("pi"))
                        return new ExprDouble(Math.PI);
                    if (name == "e")
                        return new ExprDouble(Math.E);
                }
            }

            IExpression[] initArgs = init.GetArgs();
            IExpression[] args = new IExpression[initArgs.Length];
            int doubleCount = 0;
            for (int i = 0; i < args.Length; i++)
                args[i] = SimplifyDouble(initArgs[i]);

            IExpression[] remixedArgs = new IExpression[initArgs.Length];
            for (int i = 0; i < remixedArgs.Length; i++)
            {
                IExpression arg = args[i];
                if (arg is ExprDouble)
                {
                    doubleCount++;
                    remixedArgs[remixedArgs.Length - doubleCount] = arg;
                }
                else
                    remixedArgs[i - doubleCount] = arg;
            }

            if (init is ExprSum || init is ExprMult)
            {

                double c;
                if (init is ExprSum)
                {
                    c = 0;
                    for (int i = remixedArgs.Length - doubleCount; i < remixedArgs.Length; i++)
                        c += (remixedArgs[i] as ExprDouble).Value;
                }
                else
                {
                    c = 1;
                    for (int i = remixedArgs.Length - doubleCount; i < remixedArgs.Length; i++)
                        c *= (remixedArgs[i] as ExprDouble).Value;
                }
                if (doubleCount == remixedArgs.Length)
                    return new ExprDouble(c);
                if (doubleCount > 0)
                {
                    remixedArgs[remixedArgs.Length - doubleCount] = new ExprDouble(c);
                    doubleCount--;
                }
                IExpression res = ExprUtils.GetEmptyCopy(init);
                for (int i = 0; i < remixedArgs.Length - doubleCount; i++)
                    res.AddArg(remixedArgs[i]);
                return res;
            }
            
            init = ExprUtils.GetEmptyCopy(init);
            for (int i = 0; i < args.Length; i++)
                init.AddArg(args[i]);
            if (doubleCount != args.Length)
                return init;
            if (init is ExprDiv)
                return new ExprDouble((args[0] as ExprDouble).Value / (args[1] as ExprDouble).Value);
            if (init is ExprPow)
                return new ExprDouble(Math.Pow((args[0] as ExprDouble).Value, (args[1] as ExprDouble).Value));
            if (init is ExprRegFunc)
            {
                ExprRegFunc func = init as ExprRegFunc;
                if (init.GetArgsCount() == 1)
                {
                    double arg = (args[0] as ExprDouble).Value;
                    if (func.Name == "ln")
                        return new ExprDouble(Math.Log(arg));
                    if (func.Name == "sin")
                        return new ExprDouble(Math.Sin(arg));
                    if (func.Name == "cos")
                        return new ExprDouble(Math.Cos(arg));
                    if (func.Name == "tg")
                        return new ExprDouble(Math.Tan(arg));
                    if (func.Name == "ctg")
                        return new ExprDouble(1 / Math.Tan(arg));
                    if (func.Name == "arcsin")
                        return new ExprDouble(Math.Asin(arg));
                    if (func.Name == "arccos")
                        return new ExprDouble(Math.Acos(arg));
                    if (func.Name == "arctg")
                        return new ExprDouble(Math.Atan(arg));
                    if (func.Name == "arcctg")
                        return new ExprDouble(Math.Atan(1 / arg));
                    if (func.Name == "sqrt")
                        return new ExprDouble(Math.Sqrt(arg));
                    if (func.Name == "abs")
                        return new ExprDouble(Math.Abs(arg));
                    if (func.Name == "sh")
                        return new ExprDouble(Math.Sinh(arg));
                    if (func.Name == "ch")
                        return new ExprDouble(Math.Cosh(arg));
                }
            }

            return init;
        }

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
                return SimplifyMult(new ExprMult(new ExprDouble(1), res));
            if (res is ExprPow)
                return SimplifyPow(res as ExprPow);

            return res;
        }

        private static IExpression SimplifySum(ExprSum init)
        {
            IExpression res = new ExprSum();
            double unitedConst = 0;
            foreach (IExpression _expr in init.GetArgs())
            {
                IExpression expr = _expr;
                if (expr is ExprDouble)
                {
                    double val = (expr as ExprDouble).Value;
                    unitedConst += val;
                    continue;
                }
                res.AddArg(expr);
            }

            IExpression[] resArgs = res.GetArgs();
            if (resArgs.Length == 0)
                return new ExprDouble(unitedConst);
            if (unitedConst != 0)
                res.AddArg(new ExprDouble(unitedConst));
            else if (resArgs.Length == 1)
                return resArgs[0];
            return res;
        }

        private static IExpression SimplifyMult(ExprMult init)
        {
            double unitedTopConst = 1;
            double unitedBotConst = 1;
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
                top = new ExprDouble(unitedTopConst);
            else if (unitedTopConst != 1)
                top.AddArg(new ExprDouble(unitedTopConst));
            else if (topArgs.Length == 1)
                top = topArgs[0];
            if (botArgs.Length == 0)
            {
                if (unitedBotConst == 1)
                    return top;
                else
                    bot = new ExprDouble(unitedBotConst);
            }
            else if (unitedBotConst != 1)
                bot.AddArg(new ExprDouble(unitedBotConst));
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
                if (pows[i] is ExprDouble && (pows[i] as ExprDouble).Value < 0)
                    bot.AddArg(new ExprPow(terms[i], new ExprDouble(-1 * (pows[i] as ExprDouble).Value)));
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
                top = new ExprDouble(1);
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
                    pow = new ExprDouble(-1);
                else
                    pow = new ExprDouble(1);
            }
            else if (reversal)
                pow.AddArg(new ExprDouble(-1));

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
            if (_arg2 is ExprDouble)
            {
                ExprDouble arg2 = _arg2 as ExprDouble;
                if (arg2.Value == 0)
                    return new ExprDouble(1);
                if (arg2.Value == 1)
                    return init.GetArgs()[0];
            }
            if (_arg1 is ExprDouble)
            {
                ExprDouble arg1 = _arg1 as ExprDouble;
                if (arg1.Value == 0 || arg1.Value == 1)
                    return arg1;
            }
            return init;
        }

        private static IExpression processMultArg(IExpression top, IExpression bot, ref double unitedTopConst, ref double unitedBotConst, IExpression arg)
        {
            if (arg is ExprDouble)
            {
                double val = (arg as ExprDouble).Value;
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

        public static IExpression ConvertFromExprConst(IExpression expr)
        {
            if (expr is ExprConst)
                return new ExprDouble((expr as ExprConst).Value.ToDouble());
            if (expr is ExprNaN)
                return new ExprDouble(0);
            IExpression res = ExprUtils.GetEmptyCopy(expr);
            foreach (IExpression arg in expr.GetArgs())
                res.AddArg(ConvertFromExprConst(arg));
            return res;
        }

        public static double CalcConstExpr(IExpression expr)
        {
            expr = ConvertFromExprConst(expr);
            expr = SimplifyDouble(expr);
            return (expr as ExprDouble).Value;
        }
    }
}
