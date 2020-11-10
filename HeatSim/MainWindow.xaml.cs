
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace HeatSim
{
    // expressions
    // IExpression nonLoop

    // parse
    // nice expression render
    // interface, base input
    // border cond choice
    // expression integration
    // lambda_n and X_n

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal readonly Scene1 scene1;
        internal readonly Scene2 scene2;

        public MainWindow()
        {
            InitializeComponent();

            scene1 = new Scene1(this, scene1_panel);
            scene2 = new Scene2(this, scene2_panel);
        }

        private void MainWindow_Loaded(object sender, EventArgs e)
        {
            scene1.UpdateSystemU();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (scene1.IsActive)
                scene1.UpdateSizes();
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            scene1.onDisable();
            scene2.onEnable();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            scene2.onDisable();
            scene1.onEnable();
        }
    }
}
