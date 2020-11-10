using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using InputFunction = System.Tuple<string, string, HeatSim.FunctionField.InputHandler, HeatSim.FunctionField.RenderHandler>;

namespace HeatSim
{
    internal class Scene1
    {
        private readonly MainWindow Window;
        private readonly FrameworkElement Panel;
        public bool IsActive => Panel.Visibility == Visibility.Visible;

        private readonly Panel Part1, Part2;
        private readonly Canvas SystemU, SystemV, SubstitutionW;

        private readonly List<FunctionField> FunctionFields = new List<FunctionField>();
        private static readonly int SINGLE_INPUT_HEIGHT = 30;
        private readonly ExprParser parser = new ExprParser();

        private BoundaryCond.Type left = BoundaryCond.Type.DIRICHLET;
        private BoundaryCond.Type right = BoundaryCond.Type.DIRICHLET;
        public BoundaryCond cond { get; private set; }

        public IExpression expr_a { get; private set; }
        public IExpression x_left { get; private set; }
        public IExpression x_right { get; private set; }
        public IExpression expr_t0 { get; private set; }

        private IExpression expr_f;
        private IExpression expr_mu_0 = new ExprConst(0);
        private IExpression expr_mu_1;
        private IExpression expr_phi;

        public IExpression expr_W { get; private set; }
        public IExpression expr_f_alt { get; private set; }
        public IExpression expr_phi_alt { get; private set; }

        public Scene1(MainWindow window, FrameworkElement panel)
        {
            Window = window;
            Panel = panel;
            Part1 = Window.scene1_part1;
            Part2 = Window.scene1_part2;
            SubstitutionW = Window.scene1_part3;
            SystemU = Window.system_U;
            SystemV = Window.system_V;

            //parser.AddAlias("u", 2);
            parser.AddAlias("f", 2);
            parser.AddAlias(MathAliases.ConvertName("mu") + "_0", 1);
            parser.AddAlias(MathAliases.ConvertName("mu") + "_1", 1);
            parser.AddAlias(MathAliases.ConvertName("phi"), 1);
            parser.AddAlias("x", 0);
            parser.AddAlias("t", 0);

            parser.AddAlias("a", 0);
            parser.AddAlias("l_0", 0);
            parser.AddAlias("l_1", 0);
            parser.AddAlias("t_0", 0);
            parser.AddAliases(MathAliases.GetDefaultFunctions());

            InputFunction[] names_defaults_handlers = {
                new InputFunction("a", "1", Update_a, UpdateSystemU),
                new InputFunction("l_0", "0", Update_l_0, UpdateSystemU),
                new InputFunction("l_1", "pi", Update_l_1, UpdateSystemU),
                new InputFunction("t_0", "0", Update_t_0, UpdateSystemU),
                new InputFunction("f(t, x)", "sin x", Update_f, UpdateSystemU),
                new InputFunction("mu_0(t)", "t", Update_mu_0, UpdateSystemU),
                new InputFunction("mu_1(t)", "t^2", Update_mu_1, UpdateSystemU),
                new InputFunction("phi(x)", "x", Update_phi, UpdateSystemU)
            };
            for (int i = 0; i < names_defaults_handlers.Length; i++)
            {
                string name = parser.Parse(names_defaults_handlers[i].Item1).AsString();
                string defaultVal = names_defaults_handlers[i].Item2;
                FunctionField.InputHandler handler = names_defaults_handlers[i].Item3;
                FunctionField.RenderHandler handler2 = names_defaults_handlers[i].Item4;
                FunctionFields.Add(new FunctionField(name, defaultVal, handler, handler2));
                handler(defaultVal);
            }
        }

        public void onEnable()
        {
            UpdateSystemU(); // not need in parsing
            Panel.Visibility = Visibility.Visible;
        }

        public void onDisable()
        {
            Panel.Visibility = Visibility.Hidden;
            // clean heavy objects:
            SystemU.Children.Clear();
            SystemV.Children.Clear();
            Window.scene1_part3.Children.Clear();
        }


        // TODO check no dependencies
        private void Update_a(string val)
        {
            expr_a = parser.Parse(val);
            expr_a = ExprSimplifier.Simplify(expr_a);
        }
        private void Update_l_0(string val)
        {
            x_left = parser.Parse(val);
            x_left = ExprSimplifier.Simplify(x_left);
        }
        private void Update_l_1(string val)
        {
            x_right = parser.Parse(val);
            x_right = ExprSimplifier.Simplify(x_right);
        }
        private void Update_t_0(string val)
        {
            expr_t0 = parser.Parse(val);
            expr_t0 = ExprSimplifier.Simplify(expr_t0);
        }

        private void Update_f(string val)
        {
            expr_f = parser.Parse(val);
            expr_f = ExprSimplifier.Simplify(expr_f);
        }
        private void Update_mu_0(string val)
        {
            expr_mu_0 = parser.Parse(val);
            expr_mu_0 = ExprSimplifier.Simplify(expr_mu_0);
        }
        private void Update_mu_1(string val)
        {
            expr_mu_1 = parser.Parse(val);
            expr_mu_1 = ExprSimplifier.Simplify(expr_mu_1);
        }
        private void Update_phi(string val)
        {
            expr_phi = parser.Parse(val);
            expr_phi = ExprSimplifier.Simplify(expr_phi);
        }

        private void UpdateBoundaryCond(bool isLeft, BoundaryCond.Type newType)
        {
            bool wasChanged = isLeft && left != newType || !isLeft && right != newType;
            if (isLeft)
                left = newType;
            else
                right = newType;

            if (wasChanged)
                UpdateSystemU();
        }

        public void UpdateSystemU()
        {
            IExpression utTerm = new ExprRegFunc("u_t", 0);
            IExpression uxxTerm = new ExprMult(new ExprPow(expr_a, new ExprConst("2")), new ExprRegFunc("u_xx", 0));
            uxxTerm = ExprSimplifier.Simplify(uxxTerm);
            IExpression str1 = new ExprEquality(utTerm, new ExprSum(uxxTerm, expr_f));

            IExpression leftCond = new ExprRegFunc("u", 2, x_left, new ExprRegFunc("t", 0));
            IExpression str2 = new ExprEquality(leftCond, expr_mu_0);

            IExpression rightCond = new ExprRegFunc("u", 2, x_right, new ExprRegFunc("t", 0));
            IExpression str3 = new ExprEquality(rightCond, expr_mu_1);

            IExpression startCond = new ExprRegFunc("u", 2, new ExprRegFunc("x", 0), expr_t0);
            IExpression str4 = new ExprEquality(startCond, expr_phi);

            IExpression[] expr_system_u = new IExpression[] { str1, str2, str3, str4 };

            Size size = new Size(Part2.ActualWidth / 2, Part2.ActualHeight);
            ExprRenderer.RenderSystem(SystemU, expr_system_u, size);

            UpdateW();
        }

        private void UpdateW()
        {
            cond = new BoundaryCond(left, right);
            expr_W = cond.GetZeroingFunctionW(x_left, x_right, expr_mu_0, expr_mu_1);
            expr_W = ExprSimplifier.Simplify(expr_W);
            IExpression func = new ExprRegFunc("W", 2, new ExprRegFunc("x", 0), new ExprRegFunc("t", 0));
            IExpression str = new ExprEquality(func, expr_W);
            ExprRenderer.RenderLineExpr(SubstitutionW, str, SubstitutionW.RenderSize);

            IExpression expr_Wt = ExprDeriver.Derive(expr_W, "t");
            IExpression expr_Wxx = ExprDeriver.Derive(ExprDeriver.Derive(expr_W, "x"), "x");
            IExpression expr_W0 = ExprSimplifier.Substitute(expr_W, "t", new ExprConst(0));
            expr_Wt = ExprSimplifier.Simplify(expr_Wt);
            expr_Wxx = ExprSimplifier.Simplify(expr_Wxx);
            expr_W0 = ExprSimplifier.Simplify(expr_W0);
            IExpression minusWt = new ExprMult(new ExprConst(-1), expr_Wt);
            IExpression a2Wxx = new ExprMult(new ExprPow(expr_a, new ExprConst(2)), expr_Wxx);
            expr_f_alt = new ExprSum(expr_f, minusWt, a2Wxx);
            expr_phi_alt = new ExprSum(expr_phi, new ExprMult(new ExprConst(-1), expr_W0));
            expr_f_alt = ExprSimplifier.Simplify(expr_f_alt);
            expr_phi_alt = ExprSimplifier.Simplify(expr_phi_alt);
            UpdateSystemV();
        }

        private void UpdateSystemV()
        {
            IExpression expr_mu_0 = parser.Parse("0");
            IExpression expr_mu_1 = parser.Parse("0");

            IExpression utTerm = new ExprRegFunc("V_t", 0);
            IExpression uxxTerm = new ExprMult(new ExprPow(expr_a, new ExprConst("2")), new ExprRegFunc("V_xx", 0));
            uxxTerm = ExprSimplifier.Simplify(uxxTerm);
            IExpression str1 = new ExprEquality(utTerm, new ExprSum(uxxTerm, expr_f_alt));

            IExpression leftCond = new ExprRegFunc("V", 2, x_left, new ExprRegFunc("t", 0));
            IExpression str2 = new ExprEquality(leftCond, expr_mu_0);

            IExpression rightCond = new ExprRegFunc("V", 2, x_right, new ExprRegFunc("t", 0));
            IExpression str3 = new ExprEquality(rightCond, expr_mu_1);

            IExpression startCond = new ExprRegFunc("V", 2, new ExprRegFunc("x", 0), expr_t0);
            IExpression str4 = new ExprEquality(startCond, expr_phi_alt);

            IExpression[] expr_system_v = new IExpression[] { str1, str2, str3, str4 };

            Size size = new Size(Part2.ActualWidth / 2, Part2.ActualHeight);
            ExprRenderer.RenderSystem(SystemV, expr_system_v, size);

            Window.temp_log.Text = DebugLog.ReadAll();
        }

        public void UpdateSizes()
        {
            // TODO rework using standart height restrictions and WrapPanel
            // https://stackoverflow.com/questions/717299/wpf-setting-the-width-and-height-as-a-percentage-value
            double windowWidth = Window.ActualWidth;
            double windowHeight = Window.ActualHeight - 16 - 20;
            Part1.Margin = new Thickness(5 + windowWidth * 0.05, 10 + windowHeight * 0.02, 0, 0);
            FrameworkElement element = SystemU.Parent as FrameworkElement;
            SystemU.Width = (element.ActualWidth / 100) * 40;
            SystemU.Margin = new Thickness((element.ActualWidth / 100) * 10, 0, 0, 0);

            double inputMargin = Part1.Margin.Top + Part1.Margin.Bottom;
            double systemsMargin = Part2.Margin.Top + Part2.Margin.Bottom;
            double substitMargin = SubstitutionW.Margin.Top + SubstitutionW.Margin.Bottom;

            double substitHeightFull = SubstitutionW.Height + substitMargin;
            double systemsHeightFull = windowHeight / 2;
            double inputHeightFull = windowHeight - systemsHeightFull - substitHeightFull;
            double inputHeight = inputHeightFull - inputMargin;
            inputHeight = Math.Max(SINGLE_INPUT_HEIGHT * 2, inputHeight);
            inputHeight = Math.Min(SINGLE_INPUT_HEIGHT * (FunctionFields.Count + 0.5), inputHeight);
            Part1.Height = inputHeight;
            inputHeightFull = inputHeight + inputMargin;
            double systemsHeight = windowHeight - inputHeightFull - substitHeightFull - systemsMargin;
            systemsHeight = Math.Max(100, systemsHeight);
            Part2.Height = systemsHeight;

            foreach (UIElement child in Part1.Children)
                if (child is Grid)
                    (child as Grid).Children.Clear();
            Part1.Children.Clear();
            int vertCount = (int)(inputHeight / SINGLE_INPUT_HEIGHT);
            for (int n = 0; n < FunctionFields.Count; n += vertCount)
            {
                //DataGrid doubleColumn = new DataGrid();
                //doubleColumn.RowHeight = SINGLE_INPUT_HEIGHT;
                Grid doubleColumn = new Grid();
                Part1.Children.Add(doubleColumn);
                doubleColumn.MaxHeight = vertCount * SINGLE_INPUT_HEIGHT;
                doubleColumn.VerticalAlignment = VerticalAlignment.Top;
                doubleColumn.ColumnDefinitions.Add(new ColumnDefinition());
                doubleColumn.ColumnDefinitions.Add(new ColumnDefinition());
                for (int i = 0; i < vertCount; i++)
                {
                    doubleColumn.RowDefinitions.Add(new RowDefinition());
                }
                for (int i = 0; n + i < FunctionFields.Count && i < vertCount; i++)
                {
                    FunctionField field = FunctionFields[n + i];
                    field.LeftSide.Height = SINGLE_INPUT_HEIGHT;
                    Grid.SetRow(field.LeftSide, i);
                    Grid.SetRow(field.RightSide, i);
                    doubleColumn.Children.Add(field.LeftSide);
                    doubleColumn.Children.Add(field.RightSide);
                }
            }
        }
    }
}
