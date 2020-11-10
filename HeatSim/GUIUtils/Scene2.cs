using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace HeatSim
{
    internal class Scene2
    {
        private MainWindow Window;
        private readonly FrameworkElement Panel;
        public bool IsActive => Panel.Visibility == Visibility.Visible;

        private enum Mode
        {
            INPUT, FOURIER, TIME
        };
        Mode mode = Mode.INPUT;
        FastSolution solution;
        Graph graph;
        int fourier_terms;
        int stepMsDelay = 250;
        DispatcherTimer stepTimer;

        Canvas MainCanvas;

        public Scene2(MainWindow window, FrameworkElement panel)
        {
            Window = window;
            Panel = panel;
            MainCanvas = Window.scene2_main;

            Window.check_fourier.Checked += check_fourier_Checked;
            Window.check_time.Checked += check_time_Checked;
            Window.fixingCheckBox.Checked += fixingCheckBox_Checked;
            Window.fixingCheckBox.Unchecked += fixingCheckBox_Checked;
            Window.reButton.Click += reButton_Click;
            Window.stepButton.Click += stepButton_Click;
            Window.animationButton.Click += animationButton_Click;
        }

        public void onEnable()
        {
            // TODO improve solution transmission
            Scene1 sc = Window.scene1;
            double a = ExprDoubleSimplifier.CalcConstExpr(sc.expr_a);
            double xMin = ExprDoubleSimplifier.CalcConstExpr(sc.x_left);
            double xMax = ExprDoubleSimplifier.CalcConstExpr(sc.x_right);
            double t0 = ExprDoubleSimplifier.CalcConstExpr(sc.expr_t0);
            solution = new FastSolution(sc.expr_W, sc.expr_f_alt, sc.expr_phi_alt, a, xMin, xMax, sc.cond, t0);
            graph = new Graph(MainCanvas, true, xMin, xMax);
            Refresh();
            Panel.Visibility = Visibility.Visible;
        }

        public void onDisable()
        {
            Panel.Visibility = Visibility.Hidden;
            // clean heavy objects:
            graph.Clear();
            graph = null;
            solution = null;
        }

        private void check_fourier_Checked(object sender, RoutedEventArgs e)
        {
            if (mode == Mode.INPUT)
                UpdatePrecision();
            mode = Mode.FOURIER;
            Refresh();
        }

        private void check_time_Checked(object sender, RoutedEventArgs e)
        {
            if (mode == Mode.INPUT)
                UpdatePrecision();
            mode = Mode.TIME;
            Refresh();
        }

        private void fixingCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            bool? newVal = Window.fixingCheckBox.IsChecked;
            graph?.SetFixedArea(newVal.HasValue && newVal.Value);
        }

        private void reButton_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            StopAnimation();
            if (mode == Mode.INPUT)
            {
                // set defaults
                // draw input

            }
            else if (mode == Mode.FOURIER)
            {
                fourier_terms = 1;
                graph.Draw(solution.GetX(), solution.GetInitU(fourier_terms));
            }
            else if (mode == Mode.TIME)
            {
                fourier_terms = solution.TERM_COUNT;
                solution.SetTimeStep(0);
                graph.Draw(solution.GetX(), solution.GetInitU());
            }
            UpdateInfo();
        }

        private void stepButton_Click(object sender, RoutedEventArgs e)
        {
            Step(null, null);
        }

        private void animationButton_Click(object sender, RoutedEventArgs e)
        {
            if (stepTimer == null)
            {
                Window.animationButton.Content = "Пауза";
                stepTimer = new DispatcherTimer();
                stepTimer.Tick += new EventHandler(Step);
                stepTimer.Interval = new TimeSpan(0, 0, 0, 0, stepMsDelay);
                stepTimer.Start();
            }
            else
            {
                Window.animationButton.Content = "В динамику";
                StopAnimation();
            }
        }

        private void StopAnimation()
        {
            stepTimer?.Stop();
            stepTimer = null;
        }

        private void Step(object sender, EventArgs e)
        {
            if (!IsActive)
            {
                mode = Mode.INPUT;
                stepTimer.Stop();
                stepTimer = null;
                return;
            }

            if (mode == Mode.FOURIER)
            {
                fourier_terms = fourier_terms % solution.TERM_COUNT + 1;
                graph.Draw(solution.GetX(), solution.GetInitU(fourier_terms));
            }
            else if (mode == Mode.TIME)
            {
                graph.Draw(solution.GetX(), solution.GetNextU());
            }
            UpdateInfo();
        }

        private void UpdateInfo()
        {
            Window.animInfoLabel.Content =
                "t:" + "\n   " + solution.FullTime.ToString("F3") + "\n" +
                "terms:" + "\n   " + fourier_terms + "\n" +
                "x points:" + "\n   " + (solution.X_SECTIONS + 1) + "\n" +
                "integral prec:" + "\n   " + solution.INTEGRAL_SUM_POINTS;
        }

        private void UpdatePrecision()
        {
            solution.SetPrecision(10, 0.01, 10, 100); // TODO use exact values
        }
    }
}
