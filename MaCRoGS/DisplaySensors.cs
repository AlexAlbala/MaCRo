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

        private Position central_map;
        private Position right_map;
        private Position wallback_map;
        private Position wall_map;

        private Position StartingPositionMap;
        private Position actualPositionMap;
        private Position actualPosition;

        private double mmperpixel_robotmap;
        private double mmperpixel_map;

        private Line central_line;
        private Line wall_line;
        private Line right_line;
        private Line wallback_line;

        public void UpdateS1(short value)//LEFT
        {
            Updater d = new Updater(_UpdateS1);
            this.Dispatcher.Invoke(d, value);
        }

        private void _UpdateS1(short value)//WALL_BACK
        {
            S1.Content = "WALL_BACK: " + value.ToString() + " mm";

            wallback_line.X1 = wallback_robotmap.x - (value / mmperpixel_robotmap);
            wallback_line.X2 = wallback_line.X1;
            wallback_line.Y1 = wallback_robotmap.y + 10;
            wallback_line.Y2 = wallback_robotmap.y - 10;

            wallback_line.Stroke = Brushes.Red;

            Ellipse e = new Ellipse();
            e.Width = 3;
            e.Height = 3;

            e.Stroke = Brushes.Red;
            map.Children.Add(e);

            Canvas.SetLeft(e, Canvas.GetLeft(macro) + Math.Cos(actualPositionMap.angle) * wallback_map.x - Math.Sin(actualPositionMap.angle) * (wallback_map.y) - Math.Cos(actualPositionMap.angle) * (value / mmperpixel_map));
            Canvas.SetTop(e, Canvas.GetTop(macro) + Math.Cos(actualPositionMap.angle) * wallback_map.y + Math.Sin(actualPositionMap.angle) * wallback_map.x - Math.Sin(actualPositionMap.angle) * (value / mmperpixel_map));
        }

        public void UpdateS2(short value)//WALL
        {
            Updater d = new Updater(_UpdateS2);
            this.Dispatcher.Invoke(d, value);
        }

        private void _UpdateS2(short value)//WALL
        {
            S2.Content = "WALL: " + value.ToString() + " mm";

            wall_line.X1 = wall_robotmap.x - (value / mmperpixel_robotmap);
            wall_line.X2 = wall_line.X1;
            wall_line.Y1 = wall_robotmap.y + 10;
            wall_line.Y2 = wall_robotmap.y - 10;

            wall_line.Stroke = Brushes.Blue;

            Ellipse e = new Ellipse();
            e.Width = 3;
            e.Height = 3;

            e.Stroke = Brushes.Blue;
            map.Children.Add(e);

            Canvas.SetLeft(e, Canvas.GetLeft(macro) + Math.Cos(actualPositionMap.angle) * wall_map.x - Math.Sin(actualPositionMap.angle) * (wall_map.y) - Math.Cos(actualPositionMap.angle) * (value / mmperpixel_map));
            Canvas.SetTop(e, Canvas.GetTop(macro) + Math.Cos(actualPositionMap.angle) * wall_map.y + Math.Sin(actualPositionMap.angle) * wall_map.x - Math.Sin(actualPositionMap.angle) * (value / mmperpixel_map));

        }

        public void UpdateL1(short value)//CENTRAL
        {
            Updater d = new Updater(_UpdateL1);
            this.Dispatcher.Invoke(d, value);
        }

        private void _UpdateL1(short value)//CENTRAL
        {
            L1.Content = "CENTRAL: " + value.ToString() + " mm";

            central_line.X1 = central_robotmap.x - 10;
            central_line.X2 = central_robotmap.x + 10;
            central_line.Y1 = central_robotmap.y - (value / mmperpixel_robotmap);
            central_line.Y2 = central_line.Y1;

            central_line.Stroke = Brushes.Black;

            Ellipse e = new Ellipse();
            e.Width = 3;
            e.Height = 3;

            e.Stroke = Brushes.Black;
            map.Children.Add(e);

            Canvas.SetLeft(e, Canvas.GetLeft(macro) + central_map.x * Math.Cos(actualPositionMap.angle) + Math.Sin(actualPositionMap.angle) * (value / mmperpixel_map));
            Canvas.SetTop(e, Canvas.GetTop(macro) + central_map.y*Math.Cos(actualPositionMap.angle) + Math.Sin(actualPositionMap.angle) * central_map.x - Math.Cos(actualPositionMap.angle) * (value / mmperpixel_map));
        }

        public void UpdateL2(short value)//RIGHT
        {
            Updater d = new Updater(_UpdateL2);
            this.Dispatcher.Invoke(d, value);
        }

        private void _UpdateL2(short value)//RIGHT
        {
            L2.Content = "RIGHT: " + value.ToString() + " mm";

            right_line.X1 = right_robotmap.x + (value / mmperpixel_robotmap - 10) * Math.Cos(right_robotmap.angle);
            right_line.X2 = right_robotmap.x + (value / mmperpixel_robotmap + 10) * Math.Cos(right_robotmap.angle);
            right_line.Y1 = right_robotmap.y - (value / mmperpixel_robotmap + 10) * Math.Sin(right_robotmap.angle);
            right_line.Y2 = right_robotmap.y - (value / mmperpixel_robotmap - 10) * Math.Sin(right_robotmap.angle);

            right_line.Stroke = Brushes.Brown;

            Ellipse e = new Ellipse();
            e.Width = 3;
            e.Height = 3;

            e.Stroke = Brushes.Brown;
            map.Children.Add(e);

            Canvas.SetLeft(e, Canvas.GetLeft(macro) + right_map.x * Math.Cos(actualPositionMap.angle) + Math.Sin(actualPositionMap.angle) * right_map.y + Math.Sin(actualPositionMap.angle + right_map.angle) * (value / mmperpixel_map));
            Canvas.SetTop(e, Canvas.GetTop(macro) + Math.Cos(actualPositionMap.angle) * right_map.y + Math.Sin(actualPositionMap.angle) * right_map.x - Math.Cos(actualPositionMap.angle + right_map.angle) * (value / mmperpixel_map));
        }

        public void UpdateMode(short value)//RIGHT
        {
            Updater d = new Updater(_UpdateMode);
            this.Dispatcher.Invoke(d, value);
        }

        private void _UpdateMode(short value)
        {
            switch (value)
            {
                case 0:
                    Mode.Content = "MODE: Searching for a wall";
                    break;
                case 1:
                    Mode.Content = "MODE: Following the wall";
                    break;
                default:
                    break;
            }
        }
    }
}
