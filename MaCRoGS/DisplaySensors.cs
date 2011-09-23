using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace MaCRoGS
{
    public partial class MainWindow : Window
    {
        private Position central_robotmap;
        private Position right_robotmap;
        private Position wallback_robotmap;
        private Position wall_robotmap;

        private double mmperpixel_robotmap;

        private Line central_line;
        private Line wall_line;
        private Line right_line;
        private Line wallback_line;

        /*private Path central_path;
        private Path wall_path;
        private Path right_path;
        private Path wallback_path;
         */

        private short lastCentral = -2;
        private short lastWall = -2;
        private short lastWallBack = -2;
        private short lastRight = -2;

        public void UpdateS1(short value)//LEFT
        {
            Updater d = new Updater(_UpdateS1);
            this.Dispatcher.Invoke(d, value);
        }

        private void _UpdateS1(short value)//WALL_BACK
        {
            lastWallBack = value;
            S1.Content = "WALL_BACK: " + value.ToString() + " mm";

            wallback_line.X1 = wallback_robotmap.x - (value / mmperpixel_robotmap);
            wallback_line.X2 = wallback_line.X1;
            wallback_line.Y1 = wallback_robotmap.y + 10;
            wallback_line.Y2 = wallback_robotmap.y - 10;

            wallback_line.Stroke = Brushes.Red;
        }

        public void UpdateS2(short value)//WALL
        {
            Updater d = new Updater(_UpdateS2);
            this.Dispatcher.Invoke(d, value);
        }

        private void _UpdateS2(short value)//WALL
        {
            lastWall = value;
            S2.Content = "WALL: " + value.ToString() + " mm";

            wall_line.X1 = wall_robotmap.x - (value / mmperpixel_robotmap);
            wall_line.X2 = wall_line.X1;
            wall_line.Y1 = wall_robotmap.y + 10;
            wall_line.Y2 = wall_robotmap.y - 10;

            wall_line.Stroke = Brushes.Blue;
        }

        public void UpdateL1(short value)//CENTRAL
        {
            Updater d = new Updater(_UpdateL1);
            this.Dispatcher.Invoke(d, value);
        }

        private void _UpdateL1(short value)//CENTRAL
        {
            lastCentral = value;
            L1.Content = "CENTRAL: " + value.ToString() + " mm";

            if (central_line != null)
            {
                central_line.X1 = central_robotmap.x - 10;
                central_line.X2 = central_robotmap.x + 10;
                central_line.Y1 = central_robotmap.y - (value / mmperpixel_robotmap);
                central_line.Y2 = central_line.Y1;

                central_line.Stroke = Brushes.Black;
            }

            //PathFigure figure = new PathFigure();
            //PathGeometry geometry = new PathGeometry();

            //figure.StartPoint = new Point(central_robotmap.x - central_sensor.ActualWidth / 2, central_robotmap.y - central_sensor.ActualHeight/2);

            //double angleRad = 15*Math.PI/180;

            //LineSegment s1 = new LineSegment(new Point(figure.StartPoint.X - Math.Sin(angleRad)*value / mmperpixel_robotmap, figure.StartPoint.Y - Math.Cos(angleRad)*value / mmperpixel_robotmap),true);
            //ArcSegment s2 = new ArcSegment(new Point(s1.Point.X,s1.Point.Y),new Size(30,30),15.0,false,SweepDirection.Clockwise,true);
            //LineSegment s3 = new LineSegment(new Point(s1.Point.X + central_sensor.ActualWidth,s1.Point.Y),true);

            //figure.Segments.Add(s1);
            //figure.Segments.Add(s2);
            //figure.Segments.Add(s3);

            //geometry.Figures.Add(figure);
            //central_path.Data = geometry;
        }

        public void UpdateL2(short value)//RIGHT
        {
            Updater d = new Updater(_UpdateL2);
            this.Dispatcher.Invoke(d, value);
        }

        private void _UpdateL2(short value)//RIGHT
        {
            lastRight = value;
            L2.Content = "RIGHT: " + value.ToString() + " mm";

            right_line.X1 = right_robotmap.x + (value / mmperpixel_robotmap - 10) * Math.Cos(right_robotmap.angle);
            right_line.X2 = right_robotmap.x + (value / mmperpixel_robotmap + 10) * Math.Cos(right_robotmap.angle);
            right_line.Y1 = right_robotmap.y - (value / mmperpixel_robotmap + 10) * Math.Sin(right_robotmap.angle);
            right_line.Y2 = right_robotmap.y - (value / mmperpixel_robotmap - 10) * Math.Sin(right_robotmap.angle);

            right_line.Stroke = Brushes.Brown;

        }

    }
}
