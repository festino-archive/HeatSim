using HeatSim.expressions.Managing;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HeatSim
{
    internal class Graph
    {
        private static readonly double VALUE_WIDTH = 50;
        private static readonly double VALUE_HEIGHT = 10;
        private static readonly double MARK_HALF_WIDTH = 4;
        private static readonly double SCALE_FACTOR = 0.6;
        private static readonly double ARROW_LENGTH = 8;
        private static readonly double ARROW_ANGLE = 30 / 180d * Math.PI;
        private static readonly double GRAPH_START_OFFSET = 15;
        private static readonly double GRAPH_ARROW_OFFSET = ARROW_LENGTH + 7;
        private static readonly double GRAPH_HORIZONTAL_OFFSETS = GRAPH_START_OFFSET + GRAPH_ARROW_OFFSET + VALUE_WIDTH;
        private static readonly double GRAPH_VERTICAL_OFFSETS = GRAPH_START_OFFSET + GRAPH_ARROW_OFFSET + VALUE_HEIGHT;
        private readonly Canvas Canvas;
        private bool FixedArea;

        private readonly double XMin, XMax, XLength;
        private double UMin, UMax, ULength;
        private bool init = false;

        public Graph(Canvas canvas, bool fixAreaOnInit, double xMin, double xMax)
        {
            Canvas = canvas;
            FixedArea = fixAreaOnInit;
            XMin = xMin;
            XMax = xMax;
            XLength = xMax - xMin;
        }

        public void SetFixedArea(bool fixedArea)
        {
            if (FixedArea && !fixedArea)
                init = false;
            FixedArea = fixedArea;
        }

        public void Clear()
        {
            Canvas.Children.Clear();
        }

        public void Draw(double[] xValues, double[] uValues)
        {
            if (!init)
            {
                if (FixedArea)
                    init = true;

                UMin = UMax = uValues[0];
                foreach (double u in uValues)
                    if (UMin > u)
                        UMin = u;
                    else if (u > UMax)
                        UMax = u;
                ULength = UMax - UMin;
            }

            Clear();

            Path path = new Path();
            path.Stroke = Brushes.Black;
            path.StrokeThickness = 1;
            path.Margin = new Thickness(GRAPH_START_OFFSET + VALUE_WIDTH, GRAPH_ARROW_OFFSET, GRAPH_ARROW_OFFSET, GRAPH_START_OFFSET + VALUE_HEIGHT);
            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();

            double widthFull = Canvas.ActualWidth;
            double heightFull = Canvas.ActualHeight;
            double width = widthFull - GRAPH_HORIZONTAL_OFFSETS;
            double height = heightFull - GRAPH_VERTICAL_OFFSETS;
            Point[] points = new Point[xValues.Length];
            for (int i = 0; i < points.Length; i++)
            {
                int x = (int)((xValues[i] - XMin) / XLength * width);
                int u = (int)(height - (uValues[i] - UMin) / ULength * height);
                points[i] = new Point(x, u);
            }
            //PolyBezierSegment graph = new PolyBezierSegment(points, true);
            //myPathSegmentCollection.Add(graph);
            for (int i = 1; i < points.Length; i++)
                myPathSegmentCollection.Add(new LineSegment(points[i], true));

            PathFigure myPathFigure = new PathFigure();
            myPathFigure.StartPoint = points[0];
            myPathFigure.Segments = myPathSegmentCollection;

            PathFigureCollection myPathFigureCollection = new PathFigureCollection();
            myPathFigureCollection.Add(myPathFigure);
            PathGeometry myPathGeometry = new PathGeometry();
            myPathGeometry.Figures = myPathFigureCollection;
            path.Data = myPathGeometry;

            Canvas.Children.Add(path);


            Path axes = new Path();
            axes.Stroke = Brushes.Black;
            axes.StrokeThickness = 1;
            axes.Margin = new Thickness(VALUE_WIDTH, 0, 0, VALUE_HEIGHT);
            double widthAxes = widthFull - axes.Margin.Left - axes.Margin.Right;
            double heightAxes = heightFull - axes.Margin.Top - axes.Margin.Bottom;

            PathSegmentCollection axesSegments = new PathSegmentCollection();
            PathFigure axesFigure = new PathFigure();
            axesFigure.StartPoint = new Point(0, 0);
            double arrowWidth = ARROW_LENGTH * Math.Sin(ARROW_ANGLE);
            axesSegments.Add(new LineSegment(new Point(-arrowWidth, ARROW_LENGTH), true));
            axesSegments.Add(new LineSegment(new Point(arrowWidth, ARROW_LENGTH), false));
            axesSegments.Add(new LineSegment(new Point(0, 0), true));
            axesSegments.Add(new LineSegment(new Point(0, heightAxes), true));
            axesSegments.Add(new LineSegment(new Point(widthAxes, heightAxes), true));
            axesSegments.Add(new LineSegment(new Point(widthAxes - ARROW_LENGTH, heightAxes - arrowWidth), true));
            axesSegments.Add(new LineSegment(new Point(widthAxes - ARROW_LENGTH, heightAxes + arrowWidth), false));
            axesSegments.Add(new LineSegment(new Point(widthAxes, heightAxes), true));
            //axesSegments.Add(new LineSegment(new Point(GRAPH_OFFSET + points[^1].X, points[^1].Y), true));

            Point pUmax_left = new Point(-MARK_HALF_WIDTH, GRAPH_ARROW_OFFSET);
            Point pUmax_right = new Point(MARK_HALF_WIDTH, GRAPH_ARROW_OFFSET);
            Point pUmin_left = new Point(-MARK_HALF_WIDTH, heightAxes - GRAPH_START_OFFSET);
            Point pUmin_right = new Point(MARK_HALF_WIDTH, heightAxes - GRAPH_START_OFFSET);
            Point pXmin_top = new Point(GRAPH_START_OFFSET, heightAxes - MARK_HALF_WIDTH);
            Point pXmin_bot = new Point(GRAPH_START_OFFSET, heightAxes + MARK_HALF_WIDTH);
            Point pXmax_top = new Point(widthAxes - GRAPH_ARROW_OFFSET, heightAxes - MARK_HALF_WIDTH);
            Point pXmax_bot = new Point(widthAxes - GRAPH_ARROW_OFFSET, heightAxes + MARK_HALF_WIDTH);
            axesSegments.Add(new LineSegment(pUmax_left, false));
            axesSegments.Add(new LineSegment(pUmax_right, true));
            axesSegments.Add(new LineSegment(pUmin_left, false));
            axesSegments.Add(new LineSegment(pUmin_right, true));
            axesSegments.Add(new LineSegment(pXmin_bot, false));
            axesSegments.Add(new LineSegment(pXmin_top, true));
            axesSegments.Add(new LineSegment(pXmax_bot, false));
            axesSegments.Add(new LineSegment(pXmax_top, true));
            axesFigure.Segments = axesSegments;

            PathFigureCollection axesFigureCollection = new PathFigureCollection();
            axesFigureCollection.Add(axesFigure);
            PathGeometry axesGeometry = new PathGeometry();
            axesGeometry.Figures = axesFigureCollection;
            axes.Data = axesGeometry;

            Canvas.Children.Add(axes);
            //Canvas.Children.Add(new Label() { Content = points.Length.ToString() });
            LabelGroup labelXmin = RenderDouble(Canvas, XMin, 1); // optimize
            LabelGroup labelXmax = RenderDouble(Canvas, XMax, 1);
            LabelGroup labelUmin = RenderDouble(Canvas, UMin, 1);
            LabelGroup labelUmax = RenderDouble(Canvas, UMax, 1);
            Size labelUmax_size = labelUmax.GetSize();
            labelUmax.Move(pUmax_left.X - labelUmax_size.Width + axes.Margin.Left, pUmax_left.Y - labelUmax_size.Height / 2);
            Size labelUmin_size = labelUmin.GetSize();
            labelUmin.Move(pUmin_left.X - labelUmin_size.Width + axes.Margin.Left, pUmin_left.Y - labelUmin_size.Height / 2);
            Size labelXmin_size = labelXmin.GetSize();
            labelXmin.Move(pXmin_bot.X - labelXmin_size.Width / 2 + axes.Margin.Left, pXmin_bot.Y);
            Size labelXmax_size = labelXmax.GetSize();
            labelXmax.Move(pXmax_bot.X - labelXmax_size.Width / 2 + axes.Margin.Left, pXmax_bot.Y);


            /*WriteableBitmap writeableBitmap = new WriteableBitmap(
                (int)Canvas.ActualWidth,
                (int)Canvas.ActualHeight,
                96,
                96,
                PixelFormats.Bgr32,
                null);

            using (Graphics gr = Graphics.FromImage(writeableBitmap))
            {
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            }

            Image theImage = new Image();
            theImage.Source = writeableBitmap;
            Canvas.Children.Add(theImage);*/

            /*Bitmap bm = new Bitmap((int)Canvas.Width, (int)Canvas.Height);
            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            }*/
            /*Pen shapeOutlinePen = new Pen(Brushes.Black, 2);
            shapeOutlinePen.Freeze();

            DrawingGroup dGroup = new DrawingGroup();

            using (DrawingContext dc = dGroup.Open())
            {

                dc.DrawRectangle(Brushes.Blue, shapeOutlinePen, new Rect(0, 0, 25, 25));

                dc.PushOpacity(0.5);

                dc.DrawRectangle(Brushes.Blue, shapeOutlinePen, new Rect(25, 25, 25, 25));

                dc.PushEffect(new BlurBitmapEffect(), null);
 
                dc.DrawRectangle(Brushes.Blue, shapeOutlinePen, new Rect(50, 50, 25, 25));

                dc.DrawRectangle(Brushes.Blue, shapeOutlinePen, new Rect(75, 75, 25, 25));

                dc.Pop();

                dc.DrawRectangle(Brushes.Blue, shapeOutlinePen, new Rect(100, 100, 25, 25));
            }

            Image theImage = new Image();
            DrawingImage dImageSource = new DrawingImage(dGroup);
            theImage.Source = dImageSource;
            Canvas.Children.Add(theImage);*/
        }

        private static LabelGroup RenderDouble(Canvas canvas, double d, int prec)
        {
            var valXmin = ConvertDouble(d, prec);
            IExpression exprUmax;
            if (valXmin.Item1 == "0")
                exprUmax = new ExprConst(0);
            else if (valXmin.Item2 == 0)
                exprUmax = new ExprConst(valXmin.Item1);
            else
                exprUmax = new ExprMult(new ExprConst(valXmin.Item1), new ExprPow(new ExprConst(10), new ExprConst(valXmin.Item2)));
            LabelGroup labelUmax = ExprRenderer.RenderExpr(canvas, exprUmax);
            labelUmax.Rescale(SCALE_FACTOR);
            return labelUmax;
        }

        private static Tuple<string, int> ConvertDouble(double d, int prec)
        {
            if (d == 0)
                return new Tuple<string, int>("0", 0);
            bool neg = d < 0;
            if (neg)
                d = -d;
            int pow = (int) Math.Floor(Math.Log10(d));
            double val = d / Math.Pow(10, pow);
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            string valStr = val.ToString("F" + prec, culture);
            if (neg)
                valStr = '-' + valStr;
            return new Tuple<string, int>(valStr, pow);
        }
    }
}
