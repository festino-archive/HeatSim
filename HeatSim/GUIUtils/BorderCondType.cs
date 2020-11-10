using System;

namespace HeatSim
{
    public class BoundaryCond
    {
        /// <summary>
        /// In Russian notation,
        /// <b>DIRICHLET</b> is the 1st type,
        /// <b>NEUMANN</b> is the 2nd type,
        /// <b>ROBIN</b> is the 3rd type
        /// </summary>
        public enum Type
        {
            // https://en.wikipedia.org/wiki/Boundary_value_problem#Examples
            DIRICHLET,
            NEUMANN,
            ROBIN
        }

        public readonly Type Left;
        public readonly Type Right;
        //public readonly IExpression RobinLeft;
        //public readonly IExpression RobinRight;

        public BoundaryCond(Type left, Type right)
        {
            Left = left;
            Right = right;
        }

        /// <summary>
        /// <para>1-1 and 2-2: <b>lambda_n = (pi n / l)^2</b>, n = (0),1,...,N</para>
        /// <para>1-2 and 2-1: <b>lambda_n = (pi (2n - 1) / 2l)^2</b>, n = 1,2,...,N</para>
        /// </summary>
        public double[] GetLambda(int N, double l)
        {
            double[] res = GetLambdaSqrts(N, l);
            for (int i = 0; i < res.Length; i++)
                res[i] = res[i] * res[i];
            return res;
        }

        /// <summary>
        /// Returns grid: ExprDoubled exprs
        /// <para>if l0 = 0:</para>
        /// <para>1-1 and 1-2: <b>sin sqrt(lambda_n) x</b>, n = (0),1,...,N</para>
        /// <para>2-1 and 2-2: <b>cos sqrt(lambda_n) x</b>, n = 1,2,...,N</para>
        /// </summary>
        public IExpression[] GetEigenfunctions(int N, double l0, double l)
        {
            double[] lambdaRts = GetLambdaSqrts(N, l);
            N = lambdaRts.Length;
            IExpression[] res = new IExpression[N];
            /*double[] sinCoef = new double[N];
            double[] cosCoef = new double[N];
            double l1 = l0 + l;
            double l0l1 = l0 + l1;
            if (Left == Type.DIRICHLET && Right == Type.DIRICHLET)
            {
                for (int n = 0; n < lambdaRts.Length; n++)
                {
                    double tg = Math.Tan(lambdaRts[n] * l1);
                    double normalization = l / 2 * (1 - tg) - Math.Sin(lambdaRts[n] * l) * Math.Cos(lambdaRts[n] * l0l1) * (1 + tg);
                    sinCoef[n] = 1 / normalization;
                    cosCoef[n] = tg / normalization;
                }
            }
            // TODO
            else
            {
            }

            for (int n = 0; n < N; n++)
            {
                IExpression arg = new ExprMult(new ExprDouble(lambdaRts[n]), new ExprRegFunc("x", 0));
                res[n] = new ExprSum(new ExprMult(new ExprDouble(sinCoef[n]), new ExprRegFunc("sin", 1, arg)),
                                    new ExprMult(new ExprDouble(cosCoef[n]), new ExprRegFunc("cos", 1, arg)));
            }*/
            ExprDouble normMult = new ExprDouble(2 / l);
            IExpression arg = new ExprSum(new ExprRegFunc("x", 0), new ExprMult(new ExprConst(-1), new ExprDouble(l0)));
            if (Left == Type.DIRICHLET && Right == Type.DIRICHLET || Left == Type.DIRICHLET && Right == Type.NEUMANN)
            {
                for (int n = 0; n < lambdaRts.Length; n++)
                {
                    IExpression argFull = new ExprMult(new ExprDouble(lambdaRts[n]), arg);
                    res[n] = new ExprMult(normMult, new ExprRegFunc("sin", 1, argFull));
                }
            }
            else if (Left == Type.NEUMANN && Right == Type.DIRICHLET || Left == Type.NEUMANN && Right == Type.NEUMANN)
            {
                int start = 0;
                if (Left == Type.NEUMANN && Right == Type.NEUMANN)
                {
                    start = 1;
                    res[0] = new ExprDouble(1 / l);
                }
                for (int n = start; n < lambdaRts.Length; n++)
                {
                    IExpression argFull = new ExprMult(new ExprDouble(lambdaRts[n]), arg);
                    res[n] = new ExprMult(normMult, new ExprRegFunc("cos", 1, argFull));
                }
            }
            else
            {
                // TODO
                for (int n = 0; n < lambdaRts.Length; n++)
                {
                    IExpression argFull = new ExprMult(new ExprDouble(lambdaRts[n]), arg);
                    IExpression cosMult = new ExprMult();
                    IExpression sin = new ExprRegFunc("sin", 1, argFull);
                    IExpression cos = new ExprMult(cosMult, new ExprRegFunc("cos", 1, argFull));
                    res[n] = new ExprMult(normMult, new ExprSum(sin, cos));
                }
            }
            return res;
        }

        /// <summary>
        /// Returns grid: rows are x, columns are n
        /// <para>1-1 and 1-2: <b>sin sqrt(lambda_n) x</b>, n = (0),1,...,N</para>
        /// <para>2-1 and 2-2: <b>cos sqrt(lambda_n) x</b>, n = 1,2,...,N</para>
        /// </summary>
        public double[,] GetEigenfunctionsValues(int N, double l0, double l, int sections)
        {
            double[] lambdaRts = GetLambdaSqrts(N, l);
            N = lambdaRts.Length;
            int xCount = sections + 1;
            double[,] res = new double[xCount, N];

            IExpression[] functions = GetEigenfunctions(N, l0, l);
            for (int n = 0; n < functions.Length; n++)
            {
                functions[n] = ExprDoubleSimplifier.ConvertFromExprConst(functions[n]);
                functions[n] = ExprDoubleSimplifier.SimplifyDouble(functions[n]);
            }

            for (int x = 0; x < xCount; x++)
            {
                double xVal = l0 + l * x / sections;
                for (int n = 0; n < lambdaRts.Length; n++)
                {
                    IExpression expr = FastSolution.Substitute_FromExprConst(functions[n], "x", xVal);
                    expr = ExprDoubleSimplifier.SimplifyDouble(expr);
                    res[x, n] = (expr as ExprDouble).Value;
                }
                    //res[x, n] = sinCoef[n] * Math.Sin(lambdaRts[n] * xVal) + cosCoef[n] * Math.Cos(lambdaRts[n] * xVal);
            }

            return res;
        }

        /// <summary>
        /// <para>1-1 and 2-2: <b>lambda_n = (pi n / l)^2</b>, n = (0),1,...,N</para>
        /// <para>1-2 and 2-1: <b>lambda_n = (pi (2n - 1) / 2l)^2</b>, n = 1,2,...,N</para>
        /// </summary>
        private double[] GetLambdaSqrts(int N, double l)
        {
            int startN = 1;
            if (Left == Type.NEUMANN && Right == Type.NEUMANN)
            {
                startN = 0;
                N--;
            }

            double[] res = new double[N - startN + 1];
            if (Left == Type.DIRICHLET && Right == Type.DIRICHLET || Left == Type.NEUMANN && Right == Type.NEUMANN)
                for (int i = 0; i < res.Length; i++)
                    res[i] = Math.PI / l * (i + startN);
            else if (Left == Type.DIRICHLET && Right == Type.NEUMANN || Left == Type.NEUMANN && Right == Type.DIRICHLET)
                for (int i = 0; i < res.Length; i++)
                    res[i] = Math.PI / l * (2 * (i + startN) - 1) / 2d;

            return res;
        }

        public IExpression GetZeroingFunctionW(IExpression l0, IExpression l1, IExpression mu0, IExpression mu1)
        {
            IExpression for_left;
            IExpression for_right;
            if (Left == Type.DIRICHLET && Right == Type.DIRICHLET)
            {
                // 1-1 => mu0 + (x-l0)/l (mu1-mu0) = ((l1-x) mu0 + (x-l0) mu1) / l
                IExpression l = new ExprSum(l1, new ExprMult(new ExprConst(-1), l0));
                for_left = new ExprMult(mu0, new ExprSum(l1, new ExprMult(new ExprConst(-1), new ExprRegFunc("x", 0))));
                for_right = new ExprMult(mu1, new ExprSum(new ExprRegFunc("x", 0), new ExprMult(new ExprConst(-1), l0)));
                for_left = new ExprDiv(for_left, l);
                for_right = new ExprDiv(for_right, l);
            }
            else if (Left == Type.DIRICHLET && Right == Type.NEUMANN)
            {
                // 1-2 => mu0 + (x-l0) mu1
                for_left = mu0;
                for_right = new ExprMult(mu1, new ExprSum(new ExprRegFunc("x", 0), new ExprMult(new ExprConst(-1), l0)));
            }
            else if (Left == Type.NEUMANN && Right == Type.DIRICHLET)
            {
                // 2-1 => (x-l1)mu0 + mu1
                for_left = new ExprMult(mu0, new ExprSum(new ExprRegFunc("x", 0), new ExprMult(new ExprConst(-1), l1)));
                for_right = mu1;
            }
            else if (Left == Type.NEUMANN && Right == Type.NEUMANN)
            {
                // 2-2 => ((x-l1)^2 mu0 + (x-l0)^2 mu1) / 2l^2
                IExpression l = new ExprSum(l1, new ExprMult(new ExprConst(-1), l0));
                IExpression norm = new ExprMult(new ExprConst(2), new ExprPow(l, new ExprConst(2)));
                for_left = new ExprMult(mu0, new ExprSum(new ExprRegFunc("x", 0), new ExprMult(new ExprConst(-1), l1)));
                for_right = new ExprMult(mu1, new ExprSum(new ExprRegFunc("x", 0), new ExprMult(new ExprConst(-1), l0)));
                for_left = new ExprDiv(for_left, norm);
                for_right = new ExprDiv(for_right, norm);
            }
            else
            {
                for_left = mu0;
                for_right = mu1;
            }
            return new ExprSum(for_left, for_right);
        }
    }
}
