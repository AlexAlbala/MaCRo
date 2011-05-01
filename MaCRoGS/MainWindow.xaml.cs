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
        private delegate void Updater(short value);

        private Position central_robotmap;
        private Position right_robotmap;
        private Position wallback_robotmap;
        private Position wall_robotmap;

        private Position StartingPositionMap;
        private Position actualPositionMap;

        private Rectangle macro;

        private double mmperpixel_robotmap;
        private double mmperpixel_map;

        private int mapHeight_mm;

        private Line central_line;
        private Line wall_line;
        private Line right_line;
        private Line wallback_line;


        public MainWindow()
        {
            InitializeComponent();

            coder = new Coder();
            coder.Start(this);
        }

        public void StartupSensors()
        {
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
                        break;//throw new Exception(r.Name);
                }
            }
        }

        public void Init()
        {

            mmperpixel_robotmap = 2.0;
            mapHeight_mm = 2000;


            central_line = new Line();
            central_line.Name = "c";
            robotMap.Children.Add(central_line);
            right_line = new Line();
            right_line.Name = "r";
            robotMap.Children.Add(right_line);
            wall_line = new Line();
            wall_line.Name = "w";
            robotMap.Children.Add(wall_line);
            wallback_line = new Line();
            wallback_line.Name = "l";
            robotMap.Children.Add(wallback_line);

            mmperpixel_map = mapHeight_mm / map.ActualHeight;
            macro = new Rectangle();
            macro.Width = 148 / mmperpixel_map;
            macro.Height = 235 / mmperpixel_map;
            macro.Fill = Brushes.Yellow;
            macro.Stroke = Brushes.Black;
            macro.Visibility = Visibility.Visible;




            map.Children.Add(macro);

            Canvas.SetTop(macro, map.ActualHeight / 2 - macro.ActualHeight / 2);
            Canvas.SetLeft(macro, map.ActualWidth / 2 - macro.ActualWidth / 2);

            StartingPositionMap = new Position();
            StartingPositionMap.x = (int)Canvas.GetLeft(macro);
            StartingPositionMap.y = (int)Canvas.GetTop(macro);
            StartingPositionMap.angle = 0;

            actualPositionMap = new Position();
            actualPositionMap.x = StartingPositionMap.x;
            actualPositionMap.y = StartingPositionMap.y;
            actualPositionMap.angle = StartingPositionMap.angle;

            StartupSensors();

        }

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

            Rectangle r = new Rectangle();
           
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


        public void MoveForward(short distance)
        {
            Updater d = new Updater(_MoveForward);
            this.Dispatcher.Invoke(d, distance);
        }

        public void _MoveForward(short distance)
        {
            actualPositionMap.x = actualPositionMap.x - (int)(Math.Sin(actualPositionMap.angle) * distance / mmperpixel_map);
            actualPositionMap.y = actualPositionMap.y - (int)(Math.Cos(actualPositionMap.angle) * distance / mmperpixel_map);

            Canvas.SetTop(macro, actualPositionMap.y);
            Canvas.SetLeft(macro, actualPositionMap.x);
        }

        public void MoveBackward(short distance)
        {
        }

        public void TurnLeft(short angle)
        {
            Updater d = new Updater(_TurnLeft);
            this.Dispatcher.Invoke(d, angle);
        }

        public void _TurnLeft(short angle)
        {
            actualPositionMap.angle -= angle * Math.PI / 180;
            RotateTransform rt = new RotateTransform(-1 * actualPositionMap.angle * 180 / Math.PI);
            TransformGroup tg = new TransformGroup();
            tg.Children.Add(rt);

            macro.RenderTransform = rt;
        }

        public void TurnRight(short angle)
        {
            Updater d = new Updater(_TurnRight);
            this.Dispatcher.Invoke(d, angle);
        }

        public void _TurnRight(short angle)
        {
            actualPositionMap.angle += angle * Math.PI / 180;
            RotateTransform rt = new RotateTransform(-1 * actualPositionMap.angle * 180 / Math.PI);
            TransformGroup tg = new TransformGroup();
            tg.Children.Add(rt);

            macro.RenderTransform = rt;
        }

        public void Object(short distance)
        {
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Init();
        }

        public void SetAccX(short value)
        {
        }

        public void SetAccY(short value)
        {
        }

        public void SetAccZ(short value)
        {
        }

        public void SetMagX(short value)
        {
        }

        public void SetMagY(short value)
        {
        }

        public void SetMagZ(short value)
        {
        }

        public void SetTempX(short value)
        {
        }

        public void SetTempY(short value)
        {
        }

        public void SetTempZ(short value)
        {
        }

        public void SetGyrX(short value)
        {
        }

        public void SetGyrY(short value)
        {
        }

        public void SetGyrZ(short value)
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
