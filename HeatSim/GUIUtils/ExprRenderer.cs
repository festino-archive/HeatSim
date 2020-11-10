using HeatSim.expressions.Managing;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HeatSim
{
    public static class ExprRenderer
    {
        public static double INDEX_RESCALE_RATIO = 0.55;
        public static double SUPERSCRIPT_MOVE_RATIO = 0;
        public static double SUBSCRIPT_MOVE_RATIO = 0.45;
        public static double SYSTEM_VERT_GAP = 8;
        public static double FONT_SIZE = 36;
        // Caladea, David Libre, [Gabriola], Leelawadee UI Semilight, Linux Liberine Display G
        public static FontFamily FONT_FAMILY = new FontFamily("Times New Roman");
        public static FontStyle FONT_STYLE = FontStyles.Italic;
        //public static FontFamily FONT_FAMILY = new FontFamily("Symbol");
        //public static FontStyle FONT_STYLE = FontStyles.Normal;

        public static void RenderSystem(Canvas canvas, IExpression[] system, Size size)
        {
            canvas.Children.Clear();

            Label label = NewLabel();
            label.Content = "";
            label.UpdateLayout();
            LabelGroup res = new LabelGroup(label);
            for (int i = 0; i < system.Length; i++)
                res.Children.Add(RenderExpr(canvas, system[i]));
            // TODO move, equal vertical gaps, system symbol

            double maxHeight = 0;
            for (int i = 0; i < system.Length; i++)
            {
                double height = res.Children[i].GetSize().Height;
                if (height > maxHeight)
                    maxHeight = height;
            }
            for (int i = 1; i < system.Length; i++)
                res.Children[i].Move(0, i * (SYSTEM_VERT_GAP + maxHeight));


            Label systemSymbol = NewNumberLabel();
            canvas.Children.Add(systemSymbol);
            double fullHeight = system.Length * (SYSTEM_VERT_GAP + maxHeight) - maxHeight;
            if (maxHeight > 0)
                systemSymbol.FontSize = (maxHeight / FONT_SIZE) * fullHeight * 0.9;
            systemSymbol.Content = "";// "{";
            systemSymbol.FontWeight = FontWeights.Light;
            systemSymbol.FontFamily = new FontFamily("OpenSymbol"); // Dejavu Sans Light, [Japanese Text], NSimSun, OpenSymbol
            LabelGroup systemSymbolGroup = new LabelGroup(systemSymbol);
            systemSymbolGroup.Move(-FONT_SIZE * 1.2, fullHeight * 0.07);
            res.Children.Add(systemSymbolGroup);

            Size resSize = res.GetSize();
            double ratioX = size.Width / resSize.Width;
            double ratioY = size.Height / resSize.Height;
            double ratio = Math.Min(ratioX, ratioY);
            if (0 < ratio && ratio < 1)
                res.Rescale(ratio);
        }

        public static void RenderLineExpr(Canvas canvas, IExpression expr, Size size)
        {
            canvas.Children.Clear();

            LabelGroup res = RenderExpr(canvas, expr);
            double ratioX = size.Width / res.GetSize().Width;
            double ratioY = size.Height / res.GetSize().Height;
            double ratio = Math.Min(ratioX, ratioY);
            if (0 < ratio && ratio < 1)
                res.Rescale(ratio);
        }
        internal static LabelGroup RenderExpr(Canvas canvas, IExpression expr)
        {
            if (expr is ExprNaN)
            {
                Label label = NewNumberLabel();
                canvas.Children.Add(label);
                label.FontWeight = FontWeights.UltraBold;
                label.Content = "?";
                label.UpdateLayout();
                return new LabelGroup(label);
            }
            if (expr is ExprEquality)
            {
                return RenderMultiArg(canvas, expr.GetArgs(), Priority.INIT, " = ");
            }
            if (expr is ExprConst)
            {
                Label label = NewNumberLabel();
                canvas.Children.Add(label);
                label.Content = expr.AsString();
                label.UpdateLayout();
                return new LabelGroup(label);
            }
            if (expr is ExprSum)
            {
                return RenderMultiArg(canvas, expr.GetArgs(), GetPriority(expr), " + ");
            }
            if (expr is ExprMult)
            {
                return RenderMultiArg(canvas, expr.GetArgs(), GetPriority(expr), " ");
            }
            if (expr is ExprPow)
            {
                IExpression[] args = expr.GetArgs();
                LabelGroup first = RenderExprWithBrackets(canvas, args[0], GetPriority(expr));
                LabelGroup second = RenderExpr(canvas, args[1]);
                Size firstSize = first.GetSize();
                second.Rescale(INDEX_RESCALE_RATIO);
                second.Move(firstSize.Width, -firstSize.Height * SUPERSCRIPT_MOVE_RATIO);
                first.Children.Add(second);
                return first;
            }
            if (expr is ExprDiv)
            {
                IExpression[] args = expr.GetArgs();
                LabelGroup first = RenderExpr(canvas, args[0]);
                LabelGroup second = RenderExpr(canvas, args[1]);
                Size size1 = first.GetSize(), size2 = second.GetSize();
                double width = Math.Max(size1.Width, size2.Width);
                Label label = NewNumberLabel();
                canvas.Children.Add(label);
                label.FontFamily = new FontFamily("Arial");
                label.Content = "_"; // _ —
                label.UpdateLayout();
                int count = (int) Math.Ceiling(width / label.ActualWidth);
                label.Content = new string('_', count * 2);
                label.UpdateLayout();
                width = label.ActualWidth;
                first.Move((width - size1.Width) / 2, 0);
                second.Move((width - size2.Width) / 2, 10 + size2.Height / 2);
                LabelGroup horLine = new LabelGroup(label);
                horLine.Children.Add(first);
                horLine.Children.Add(second);
                horLine.Move(0, -FONT_SIZE / 2);
                return horLine;
                //return RenderMultiArg(canvas, expr.GetArgs(), GetPriority(expr), " / ");
            }
            if (expr is ExprRegFunc)
            {
                string[] parts = (expr as ExprRegFunc).Name.Split("_");
                Label label = NewLabel();
                canvas.Children.Add(label);
                label.Content = parts[0];
                label.UpdateLayout();
                LabelGroup main = new LabelGroup(label);
                for (int i = 1; i < parts.Length; i++)
                {
                    if (parts[i] == "")
                        continue;
                    Label lowIndex;
                    if (char.IsDigit(parts[i][0]))
                        lowIndex = NewNumberLabel();
                    else
                        lowIndex = NewLabel();
                    canvas.Children.Add(lowIndex);
                    lowIndex.Content = parts[i];
                    lowIndex.UpdateLayout();
                    LabelGroup lowIndexGroup = new LabelGroup(lowIndex);
                    Size mainSize = main.GetSize();
                    lowIndexGroup.Rescale(INDEX_RESCALE_RATIO);
                    lowIndexGroup.Move(mainSize.Width, mainSize.Height * (1 - SUBSCRIPT_MOVE_RATIO));
                    main.Children.Add(lowIndexGroup);
                }
                // LOW INDECES
                if (expr.GetArgsCount() > 0)
                {
                    Label labelOpenBracket = NewNumberLabel();
                    LabelGroup groupOpenBracket = new LabelGroup(labelOpenBracket);
                    main.Children.Add(groupOpenBracket);
                    canvas.Children.Add(labelOpenBracket);
                    labelOpenBracket.Content = "(";
                    labelOpenBracket.UpdateLayout();
                    groupOpenBracket.Move(main.GetSize().Width, 0);

                    LabelGroup args = RenderMultiArg(canvas, expr.GetArgs(), Priority.INIT, ", ");
                    args.Move(main.GetSize().Width, 0);
                    main.Children.Add(args);

                    Label labelEndBracket = NewNumberLabel();
                    canvas.Children.Add(labelEndBracket);
                    LabelGroup groupEndBracket = new LabelGroup(labelEndBracket);
                    labelEndBracket.Content = ")";
                    labelEndBracket.UpdateLayout();
                    groupEndBracket.Move(main.GetSize().Width, 0);
                    main.Children.Add(groupEndBracket);
                }
                return main;
            }
            Label res = NewLabel();
            canvas.Children.Add(res);
            res.Content = "!!!";
            res.UpdateLayout();
            return new LabelGroup(res);
        }

        private static LabelGroup RenderExprWithBrackets(Canvas canvas, IExpression rendering, Priority renderer)
        {
            LabelGroup main = RenderExpr(canvas, rendering);
            if (NeedBrackets(rendering, renderer))
            {
                Label labelOpenBracket = NewNumberLabel();
                LabelGroup group = new LabelGroup(labelOpenBracket);
                labelOpenBracket.Content = "(";
                canvas.Children.Add(labelOpenBracket);
                labelOpenBracket.UpdateLayout();

                Label labelEndBracket = NewNumberLabel();
                LabelGroup groupEndBracket = new LabelGroup(labelEndBracket);
                labelEndBracket.Content = ")";
                canvas.Children.Add(labelEndBracket);
                labelEndBracket.UpdateLayout();

                main.Move(labelOpenBracket.RenderSize.Width, 0);
                groupEndBracket.Move(labelOpenBracket.RenderSize.Width + main.GetSize().Width, 0);
                group.Children.Add(main);
                group.Children.Add(groupEndBracket);
                main = group;
            }
            return main;
        }
        private static LabelGroup RenderMultiArg(Canvas canvas, IExpression[] args, Priority renderer, string sep)
        {
            if (args.Length == 0)
                return RenderExpr(canvas, new ExprNaN());
            if (args.Length == 1)
                return RenderExpr(canvas, args[0]);

            LabelGroup main = RenderExprWithBrackets(canvas, args[0], renderer);
            for (int i = 1; i < args.Length; i++)
            {
                if (i > 0)
                {
                    Label labelMult = NewLabel();
                    canvas.Children.Add(labelMult);
                    LabelGroup groupMult = new LabelGroup(labelMult);
                    labelMult.Content = sep;
                    labelMult.UpdateLayout();
                    groupMult.Move(main.GetSize().Width, 0);
                    main.Children.Add(groupMult);
                }
                LabelGroup argGroup = RenderExprWithBrackets(canvas, args[i], renderer);
                argGroup.Move(main.GetSize().Width, 0);
                main.Children.Add(argGroup);
            }
            return main;
        }

        private static Label NewLabel()
        {
            Label label = new Label();
            label.FontSize = FONT_SIZE;
            label.FontFamily = FONT_FAMILY;
            label.FontStyle = FONT_STYLE;
            label.Margin = new Thickness(0, 0, 0, 0);
            label.Padding = new Thickness(0, 0, 0, 0);
            return label;
        }

        private static Label NewNumberLabel()
        {
            Label label = new Label();
            label.FontSize = FONT_SIZE;
            label.FontFamily = FONT_FAMILY;
            label.FontStyle = FontStyles.Normal;
            label.Margin = new Thickness(0, 0, 0, 0);
            label.Padding = new Thickness(0, 0, 0, 0);
            return label;
        }

        private static bool NeedBrackets(IExpression expr, Priority renderer)
        {
            Priority rendering = GetPriority(expr);
            if (rendering < renderer)
                return true;
            return false;
        }
        private static bool NeedBrackets(IExpression expr, IExpression renderer)
        {
            return NeedBrackets(expr, GetPriority(expr));
        }
        public static string AsString(IExpression expr, Priority renderer)
        {
            if (NeedBrackets(expr, renderer))
                return "(" + expr.AsString() + ")";
            return expr.AsString();
        }
        public static Priority GetPriority(IExpression expr)
        {
            // check static field "PRIORITY"?
            if (expr is ExprNaN)
                return ExprNaN.PRIORITY;
            if (expr is ExprEquality)
                return ExprEquality.PRIORITY;
            if (expr is ExprConst)
                return ExprConst.PRIORITY;
            if (expr is ExprDouble)
                return ExprDouble.PRIORITY;
            if (expr is ExprSum)
                return ExprSum.PRIORITY;
            if (expr is ExprMult)
                return ExprMult.PRIORITY;
            if (expr is ExprDiv)
                return ExprDiv.PRIORITY;
            if (expr is ExprPow)
                return ExprPow.PRIORITY;
            if (expr is ExprRegFunc)
                return ExprRegFunc.PRIORITY;
            if (expr is ExprDerivative)
                return ExprDerivative.PRIORITY;
            return Priority.INIT;
        }
    }

    /*
            DrawingGroup group = new DrawingGroup();
            Draw(group, "u = sin sin x");
            return new DrawingImage(group);
     */

    /*private static void Draw(DrawingGroup group, string str)
    {
        /*
        FormattedText formattedText = new FormattedText(
               expr.AsString(),
               CultureInfo.GetCultureInfo("en-us"),
               FlowDirection.LeftToRight,
               new Typeface("Verdana"),
               32,
               Brushes.Black);
        formattedText.MaxTextWidth = size.Width;
        formattedText.MaxTextHeight = size.Height;
        formattedText.BuildGeometry(new Point(0, 0));*/
    /*// https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.glyphrundrawing?view=netcore-3.1
    List<ushort> ushortStr = new List<ushort>();
    foreach (char c in str)
        ushortStr.Add(c);
    List<double> lens = new List<double>();
    foreach (char c in str)
        lens.Add(9);
    GlyphRun theGlyphRun = new GlyphRun(
            new GlyphTypeface(new Uri(@"C:\Windows\Fonts\Times.ttf")),
            0,
            false,
            13.333333333333334,
            2,
            ushortStr.ToArray(),
            new Point(0, 12.29),
            lens.ToArray(),
            null,
            null,
            null,
            null,
            null,
            null
    );

    group.Children.Add(new GlyphRunDrawing(Brushes.Black, theGlyphRun));*/

    /*FormattedText formattedText = new FormattedText(
           str,
           CultureInfo.GetCultureInfo("en-us"),
           System.Windows.FlowDirection.LeftToRight,
           new Typeface(
               new FontFamily("David Libre"), // Caladea, David Libre, [Gabriola], Leelawadee UI Semilight, Linux Liberine Display G
               FontStyles.Normal,
               FontWeights.Light,
               FontStretches.Normal),
           5,
           Brushes.Black,
           1);
    //formattedText.MaxTextWidth = 100;
    //formattedText.MaxTextHeight = 50;
    //formattedText.SetFontSize(1);
    group.Children.Add(new GeometryDrawing(Brushes.Black, new Pen(), formattedText.BuildGeometry(new Point(0, 0))));
            group.DrawText(formattedText, new Point(10, 0));
        }
*/
}
