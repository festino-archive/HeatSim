using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace HeatSim.expressions.Managing
{
    class LabelGroup
    {
        public readonly List<LabelGroup> Children = new List<LabelGroup>();
        public readonly Label Own;
        private Point point = new Point(0, 0);

        public LabelGroup(Label label)
        {
            Own = label;
        }

        public void Move(double hor, double vert)
        {
            point = new Point(point.X + hor, point.Y + vert);
            Own.Margin = new Thickness(
                Own.Margin.Left + hor,
                Own.Margin.Top + vert, 
                Own.Margin.Right + hor, 
                Own.Margin.Bottom + vert
            );
            foreach (LabelGroup group in Children)
                group.Move(hor, vert);
        }

        public void Rescale(double multiplier)
        {
            point = new Point(point.X * multiplier, point.Y * multiplier);
            Own.FontSize *= multiplier;
            Own.Margin = new Thickness(
                Own.Margin.Left * multiplier,
                Own.Margin.Top * multiplier,
                Own.Margin.Right * multiplier,
                Own.Margin.Bottom * multiplier
            );
            Own.UpdateLayout();
            foreach (LabelGroup group in Children)
                group.Rescale(multiplier);
        }

        public Point GetPosition()
        {
            return point;
        }

        public Size GetSize()
        {
            Point[] corners = GetBoundingBox();
            return new Size(corners[1].X - corners[0].X, corners[1].Y - corners[0].Y);
        }

        public Point[] GetBoundingBox()
        {
            Point[] corners = GetSimpleBoundingBox();
            foreach (LabelGroup group in Children)
            {
                Point[] childCorners = group.GetBoundingBox();
                //DebugLog.WriteLine(corners[0] + " " + corners[1]);
                //DebugLog.WriteLine(" x " + childCorners[0] + " " + childCorners[1] + " (" + group.Own.Content + ")");
                if (corners[0].X > childCorners[0].X)
                    corners[0].X = childCorners[0].X;
                if (corners[0].Y > childCorners[0].Y)
                    corners[0].Y = childCorners[0].Y;

                if (corners[1].X < childCorners[1].X)
                    corners[1].X = childCorners[1].X;
                if (corners[1].Y < childCorners[1].Y)
                    corners[1].Y = childCorners[1].Y;
                //DebugLog.WriteLine("-> " + corners[0] + " " + corners[1]);
            }
            return corners;
        }

        private Point[] GetSimpleBoundingBox()
        {
            Point min = new Point(point.X, point.Y);
            Point max = new Point(point.X + Own.ActualWidth, point.Y + Own.ActualHeight);
            //Point max = new Point(point.X + Own.RenderSize.Width, point.Y + Own.RenderSize.Height);
            return new Point[] { min, max };
        }
    }
}
