using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MaCRoGS.Communications;

namespace MaCRoGS
{   
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Coder coder;
        private delegate void Updater(ushort value);

        private int mapWidth;
        private int mapHeight;

        private double mmperpx_x;
        private double mmperpx_y;

        private Position central_robotmap;
        private Position right_robotmap;
        private Position wallback_robotmap;
        private Position wall_robotmap;

        private int mmperpixel_robotmap;

        private Line central_line;
        private Line wall_line;
        private Line right_line;
        private Line wallback_line;


        public MainWindow()
        {
            InitializeComponent();

            mapWidth = 2000;
            mapHeight = 2000;

            this.Init();
            coder = new Coder();
            coder.Start(this);




        }

        public void Init()
        {
            mmperpx_x = mapWidth / map.ActualWidth;
            mmperpx_y = mapHeight / map.ActualHeight;

            mmperpixel_robotmap = 2;

            central_line = new Line();
            central_line.Name = "c";
            right_line = new Line();
            right_line.Name = "r";
            wall_line = new Line();
            wall_line.Name = "w";
            wallback_line = new Line();
            wallback_line.Name = "l";

            IEnumerable<Rectangle> rect = robotMap.Children.OfType<Rectangle>();
            foreach (Rectangle r in rect)
            {
                switch (r.Name)
                {
                    case "central_sensor":
                        central_robotmap = new Position();

                        central_robotmap.angle = 0;
                        central_robotmap.x = (int)(Canvas.GetLeft(r) + r.ActualWidth / 2);
                        central_robotmap.y = (int)Canvas.GetTop(r);
                        break;
                    case "right_sensor":
                        right_robotmap = new Position();

                        TransformGroup rotation1 = (TransformGroup)r.RenderTransform;
                        IEnumerable<RotateTransform> rot1 = rotation1.Children.OfType<RotateTransform>();

                        right_robotmap.angle = rot1.ElementAt<RotateTransform>(0).Angle * Math.PI / 180;

                        right_robotmap.x = (int)(Canvas.GetLeft(r));
                        right_robotmap.y = (int)(Canvas.GetTop(r));


                        break;
                    case "wallback_sensor":
                        wallback_robotmap = new Position();

                        TransformGroup rotation2 = (TransformGroup)r.RenderTransform;
                        IEnumerable<RotateTransform> rot2 = rotation2.Children.OfType<RotateTransform>();

                        wallback_robotmap.angle = rot2.ElementAt<RotateTransform>(0).Angle * Math.PI / 180;

                        wallback_robotmap.x = (int)Canvas.GetLeft(r);
                        wallback_robotmap.y = (int)Canvas.GetTop(r);


                        wallback_robotmap.x = (int)(Canvas.GetLeft(r));
                        wallback_robotmap.y = (int)(Canvas.GetTop(r));
                        break;
                    case "wall_sensor":
                        wall_robotmap = new Position();

                        TransformGroup rotation3 = (TransformGroup)r.RenderTransform;
                        IEnumerable<RotateTransform> rot3 = rotation3.Children.OfType<RotateTransform>();

                        wall_robotmap.angle = rot3.ElementAt<RotateTransform>(0).Angle * Math.PI / 180;

                        wall_robotmap.x = (int)Canvas.GetLeft(r);
                        wall_robotmap.y = (int)Canvas.GetTop(r);

                        wall_robotmap.x = (int)(Canvas.GetLeft(r));
                        wall_robotmap.y = (int)(Canvas.GetTop(r) + r.ActualWidth / 2);


                        break;
                    default:
                        throw new Exception(r.Name);
                }
            }
        }

        public void UpdateS1(ushort value)//LEFT
        {
            Updater d = new Updater(_UpdateS1);
            this.Dispatcher.Invoke(d, value);
        }

        private void _UpdateS1(ushort value)//LEFT
        {
            S1.Content = "WALL_BACK: " + value.ToString() + " mm";

            robotMap.Children.Remove(wallback_line);

            wallback_line.X1 = wallback_robotmap.x - (value / mmperpixel_robotmap);
            wallback_line.X2 = wallback_line.X1;
            wallback_line.Y1 = wallback_robotmap.y + 10;
            wallback_line.Y2 = wallback_robotmap.y - 10;

            wallback_line.Stroke = Brushes.Red;
            robotMap.Children.Add(wallback_line);
        }

        public void UpdateS2(ushort value)//WALL
        {
            Updater d = new Updater(_UpdateS2);
            this.Dispatcher.Invoke(d, value);
        }

        private void _UpdateS2(ushort value)//WALL
        {
            S2.Content = "WALL: " + value.ToString() + " mm";

            robotMap.Children.Remove(wall_line);

            wall_line.X1 = wall_robotmap.x - (value / mmperpixel_robotmap);
            wall_line.X2 = wall_line.X1;
            wall_line.Y1 = wall_robotmap.y + 10;
            wall_line.Y2 = wall_robotmap.y - 10;

            wall_line.Stroke = Brushes.Blue;
            robotMap.Children.Add(wall_line);
        }

        public void UpdateL1(ushort value)//CENTRAL
        {
            Updater d = new Updater(_UpdateL1);
            this.Dispatcher.Invoke(d, value);
        }

        private void _UpdateL1(ushort value)//CENTRAL
        {
            L1.Content = "CENTRAL: " + value.ToString() + " mm";

            robotMap.Children.Remove(central_line);

            central_line.X1 = central_robotmap.x - 10;
            central_line.X2 = central_robotmap.x + 10;
            central_line.Y1 = central_robotmap.y - (value / mmperpixel_robotmap);
            central_line.Y2 = central_line.Y1;

            central_line.Stroke = Brushes.Black;
            robotMap.Children.Add(central_line);
        }

        public void UpdateL2(ushort value)//RIGHT
        {
            Updater d = new Updater(_UpdateL2);
            this.Dispatcher.Invoke(d, value);
        }

        private void _UpdateL2(ushort value)//RIGHT
        {
            L2.Content = "RIGHT: " + value.ToString() + " mm";

            //right_line = new Line();
            robotMap.Children.Remove(right_line);

            right_line.X1 = right_robotmap.x + (value / mmperpixel_robotmap - 10) * Math.Cos(right_robotmap.angle);
            right_line.X2 = right_robotmap.x + (value / mmperpixel_robotmap + 10) * Math.Cos(right_robotmap.angle);
            right_line.Y1 = right_robotmap.y - (value / mmperpixel_robotmap + 10) * Math.Sin(right_robotmap.angle);
            right_line.Y2 = right_robotmap.y - (value / mmperpixel_robotmap - 10) * Math.Sin(right_robotmap.angle);

            right_line.Stroke = Brushes.Brown;
            robotMap.Children.Add(right_line);
        }

        public void UpdateMode(ushort value)//RIGHT
        {
            Updater d = new Updater(_UpdateMode);
            this.Dispatcher.Invoke(d, value);
        }

        private void _UpdateMode(ushort value)
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


        public void MoveForward(ushort distance)
        {

        }

        public void MoveBackward(ushort distance)
        {
        }

        public void TurnLeft(ushort angle)
        {
        }

        public void TurnRight(ushort angle)
        {
        }

        public void Object(ushort distance)
        {
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
    }

    public class Position
    {
        private int _x;
        private int _y;
        private double _angle;

        public int x { get { return _x; } set { _x = value; } }
        public int y { get { return _y; } set { _y = value; } }
        public double angle { get { return _angle; } set { _angle = value; } }
    }
}
