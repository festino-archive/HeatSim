using System.Windows;
using System.Windows.Controls;

namespace HeatSim
{
    internal class FunctionField
    {
        internal delegate void InputHandler(string newVal);
        internal delegate void RenderHandler();

        internal string Name;
        internal Label LeftSide;
        internal TextBox RightSide;
        internal InputHandler Input_Handler;
        internal RenderHandler Render_Handler;

        public FunctionField(string name, InputHandler handler, RenderHandler handler2)
        {
            Name = name;
            Input_Handler = handler;
            Render_Handler = handler2;
            LeftSide = new Label();
            LeftSide.Content = name + " = ";
            RightSide = new TextBox();
            RightSide.Text = "1";
            InitObjects();
        }

        public FunctionField(string name, string defaultValue, InputHandler handler, RenderHandler handler2)
        {
            Name = name;
            Input_Handler = handler;
            Render_Handler = handler2;
            LeftSide = new Label();
            LeftSide.Content = name + " = ";
            RightSide = new TextBox();
            RightSide.Text = defaultValue;
            InitObjects();
        }

        public FunctionField(string name, Label leftSide, TextBox rightSide, InputHandler handler, RenderHandler handler2)
        {
            Name = name;
            Input_Handler = handler;
            Render_Handler = handler2;
            LeftSide = leftSide;
            RightSide = rightSide;
            InitObjects();
        }

        private void InitObjects()
        {
            LeftSide.HorizontalAlignment = HorizontalAlignment.Right;
            RightSide.HorizontalAlignment = HorizontalAlignment.Left;
            LeftSide.VerticalAlignment = VerticalAlignment.Top;
            RightSide.VerticalAlignment = VerticalAlignment.Top;
            RightSide.MinWidth = 50;
            RightSide.MaxWidth = 300;
            RightSide.MaxHeight = 40;
            RightSide.Margin = new Thickness(0, 5, 0, 0);
            Grid.SetColumn(LeftSide, 0);
            Grid.SetColumn(RightSide, 1);

            RightSide.TextChanged += RightSide_TextChanged;
        }

        private void RightSide_TextChanged(object sender, TextChangedEventArgs e)
        {
            Input_Handler(RightSide.Text);
            Render_Handler();
        }
    }
}
