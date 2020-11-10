using System;
using System.Collections.Generic;

namespace HeatSim
{
    class FastSolution
    {
        private readonly double A, A_2;
        private readonly double X_LEFT, X_RIGHT;
        private readonly double X_LENGTH;
        private readonly double START_T;
        private readonly BoundaryCond COND;
        private readonly IExpression W_xt, F_xt, PHI_x;


        public int TERM_COUNT { get; private set; }
        public int INTEGRAL_SUM_POINTS { get; private set; }
        public int X_SECTIONS { get; private set; }
        public double TIME_STEP { get; private set; }

        private int time_step_count;
        private double time_elapsed;
        public double FullTime { get; private set; }


        private double[] X_points;
        private double[] Lambda_n;
        private IExpression[] X_n;
        private double[,] X_n_values;

        private IExpression[] W_t_atX;
        private double[] phi_n;
        private IExpression[] f_t_n;
        private List<double>[] f_n;

        private double[] a2_Lambda_n;
        private List<double>[] e_a2_lambda_n_t;

        public FastSolution(IExpression w, IExpression f_alt, IExpression phi_alt, double a, double xMin, double xMax, BoundaryCond boundaryCond, double t0)
        {
            A = a;
            A_2 = A * A;
            W_xt = w;
            F_xt = f_alt;
            PHI_x = phi_alt;
            X_LEFT = xMin;
            X_RIGHT = xMax;
            X_LENGTH = xMax - xMin;
            START_T = t0;
            COND = boundaryCond;
        }

        public void SetPrecision(int xSections, double timeStep, int integralSections, int fourierTermCount)
        {
            TIME_STEP = timeStep;
            TERM_COUNT = fourierTermCount;

            if (integralSections <= 0)
                integralSections = 1;
            INTEGRAL_SUM_POINTS = integralSections;

            if (xSections <= 0)
                xSections = 1;
            X_SECTIONS = xSections;
            X_points = new double[X_SECTIONS + 1];
            for (int x = 0; x < X_points.Length; x++)
                X_points[x] = X_LEFT + X_LENGTH * x / xSections;
            Lambda_n = COND.GetLambda(fourierTermCount, X_LENGTH);
            X_n = COND.GetEigenfunctions(fourierTermCount, X_LEFT, X_LENGTH);
            X_n_values = COND.GetEigenfunctionsValues(fourierTermCount, X_LEFT, X_LENGTH, X_SECTIONS);

            W_t_atX = new IExpression[X_points.Length];
            for (int x = 0; x < X_points.Length; x++)
            {
                W_t_atX[x] = Substitute_FromExprConst(W_xt, "x", X_points[x]);
                W_t_atX[x] = ExprDoubleSimplifier.SimplifyDouble(W_t_atX[x]);
                W_t_atX[x] = ExprDoubleSimplifier.Simplify(W_t_atX[x]);
            }
            phi_n = new double[TERM_COUNT];
            for (int n = 0; n < TERM_COUNT; n++)
                phi_n[n] = Integrate(new ExprMult(PHI_x, X_n[n]), "x", X_LEFT, X_RIGHT, xSections * integralSections);
            f_t_n = new IExpression[TERM_COUNT];
            for (int n = 0; n < TERM_COUNT; n++)
                f_t_n[n] = IntegrateExpr(new ExprMult(F_xt, X_n[n]), "x", X_LEFT, X_RIGHT, xSections * integralSections);
            f_n = new List<double>[TERM_COUNT];
            for (int n = 0; n < TERM_COUNT; n++)
            {
                f_n[n] = new List<double>();
                f_n[n].Add(Integrate(f_t_n[n], "t", START_T, START_T + 1, 1)); // getValue
            }

            a2_Lambda_n = new double[Lambda_n.Length];
            for (int n = 0; n < TERM_COUNT; n++)
                a2_Lambda_n[n] = A_2 * Lambda_n[n];
            e_a2_lambda_n_t = new List<double>[TERM_COUNT];
            for (int n = 0; n < TERM_COUNT; n++)
            {
                e_a2_lambda_n_t[n] = new List<double>();
                e_a2_lambda_n_t[n].Add(1);
            }

            SetTimeStep(0);
        }

        public void SetTimeStep(int timeStep)
        {
            time_step_count = timeStep;
            time_elapsed = TIME_STEP * time_step_count;
            FullTime = START_T + time_elapsed;
        }

        public double[] GetX()
        {
            return X_points;
        }

        // U = W + sum (T_n, X_n)
        //T_n(t) = phi_n * e_a2_lambda_n_t + integral from t0 to t (f_n(tau) e_a2_lambda_n_(tau - t) dtau)
        public double[] GetNextU()
        {
            time_step_count++;
            time_elapsed = TIME_STEP * time_step_count;
            FullTime = START_T + time_elapsed;
            int m = time_step_count * INTEGRAL_SUM_POINTS;

            //calc helpers
            for (int n = 0; n < TERM_COUNT; n++)
            {
                List<double> f = f_n[n];
                List<double> e = e_a2_lambda_n_t[n];
                double lambda = -a2_Lambda_n[n];
                for (int i = m - INTEGRAL_SUM_POINTS + 1; i <= m; i++)
                {
                    double time = TIME_STEP / INTEGRAL_SUM_POINTS * i;
                    e.Add(Math.Exp(lambda * time));
                    f.Add(GetValue(f_t_n[n], "t", START_T + time)); // because of memory integration / expanding integration
                }
            }

            return GetCurrentU();
        }

        public double[] GetCurrentU()
        {
            int m = time_step_count * INTEGRAL_SUM_POINTS;
            double ministep = TIME_STEP / INTEGRAL_SUM_POINTS;

            double[] res = new double[X_points.Length];
            for (int x = 0; x < X_points.Length; x++)
            {
                res[x] = 0;
                for (int n = 0; n < TERM_COUNT; n++)
                {
                    List<double> exps = e_a2_lambda_n_t[n];
                    List<double> f = f_n[n];
                    double integral = 0;
                    for (int j = 0; j <= m; j++)
                        integral += f[j] * exps[m - j] * ministep;
                    integral /= time_elapsed;
                    res[x] += (phi_n[n] * exps[m] + integral) * X_n_values[x, n];
                }
                res[x] += GetValue(W_t_atX[x], "t", FullTime);
            }
            return res;
        }

        public double[] GetInitU()
        {
            return GetInitU(TERM_COUNT);
        }

        public double[] GetInitU(int terms)
        {
            if (terms < 1)
                terms = 1;
            if (terms > TERM_COUNT)
                terms = TERM_COUNT;

            double[] res = new double[X_points.Length];
            for (int x = 0; x < X_points.Length; x++)
            {
                res[x] = 0;
                for (int n = 0; n < terms; n++)
                    res[x] += phi_n[n] * e_a2_lambda_n_t[n][0] * X_n_values[x, n];
            }
            return res;
        }

        public static double GetValue(IExpression expr, string varName, double at)
        {
            return Integrate(expr, varName, at, at + 1, 1);
        }

        public static double Integrate(IExpression expr, string varName, double from, double to, int sections)
        {
            double res = 0;
            // to calculating - optimization
            // calc
            //if (varName == "t")
            //    DebugLog.WriteLine(expr.AsString()+" "+ varName + "\n -> " + IntegrateExpr(expr, varName, from, to, sections).AsString() + " (" + IntegrateExpr(expr, varName, from, to, sections)+")");
            res = (IntegrateExpr(expr, varName, from, to, sections) as ExprDouble).Value;

            return res;
        }

        public static IExpression IntegrateExpr(IExpression expr, string varName, double from, double to, int sections)
        {
            double len = to - from;
            IExpression res = new ExprSum();
            for (int i = 0; i < sections; i++)
                res.AddArg(Substitute_FromExprConst(expr, varName, from + len * i / sections));
            //DebugLog.WriteLine(res.AsString());
            res = new ExprMult(new ExprDouble(1 / len), res);
            //DebugLog.WriteLine(res.AsString());
            res = ExprDoubleSimplifier.SimplifyDouble(res);
            //DebugLog.WriteLine(res.AsString());
            res = ExprDoubleSimplifier.Simplify(res);
            //DebugLog.WriteLine(res.AsString());
            return res;
        }

        public static IExpression Substitute_FromExprConst(IExpression expr, string varName, double value)
        {
            if (expr is ExprConst)
                return new ExprDouble((expr as ExprConst).Value.ToDouble());
            if (expr is ExprRegFunc && expr.GetArgsCount() == 0 && (expr as ExprRegFunc).Name == varName)
                return new ExprDouble(value);
            IExpression res = ExprUtils.GetEmptyCopy(expr);
            foreach (IExpression arg in expr.GetArgs())
                res.AddArg(Substitute_FromExprConst(arg, varName, value));
            return res;
        }
    }
}
