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
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace MaCRoGS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Coder coder;
        private delegate void Updater(short value);
        private delegate void UpdaterD(double value);

        public MainWindow()
        {
            InitializeComponent();

            coder = new Coder();
            coder.Start(this);

        }

        public void StartupSensors()
        {
            central_robotmap = new Position();

            central_robotmap.angle = 0;
            central_robotmap.x = (int)(Canvas.GetLeft(central_sensor) + central_sensor.ActualWidth / 2);
            central_robotmap.y = (int)Canvas.GetTop(central_sensor);

            right_robotmap = new Position();

            RotateTransform t_rrmap = (RotateTransform)right_sensor.RenderTransform;

            right_robotmap.angle = t_rrmap.Angle * Math.PI / 180;

            right_robotmap.x = (int)(Canvas.GetLeft(right_sensor));
            right_robotmap.y = (int)(Canvas.GetTop(right_sensor));

            wallback_robotmap = new Position();

            RotateTransform t_wbrmap = (RotateTransform)wallback_sensor.RenderTransform;

            wallback_robotmap.angle = t_wbrmap.Angle * Math.PI / 180;

            wallback_robotmap.x = (int)Canvas.GetLeft(wallback_sensor);
            wallback_robotmap.y = (int)Canvas.GetTop(wallback_sensor);

            wall_robotmap = new Position();

            RotateTransform t_wrmap = (RotateTransform)wall_sensor.RenderTransform;

            wall_robotmap.angle = t_wrmap.Angle * Math.PI / 180;


            wall_robotmap.x = (int)(Canvas.GetLeft(wall_sensor));
            wall_robotmap.y = (int)(Canvas.GetTop(wall_sensor) + wall_sensor.ActualWidth / 2);



            central_map = new Position();

            central_map.angle = 0;
            central_map.x = (int)(Canvas.GetLeft(central_sensor1) + central_sensor1.ActualWidth / 2);
            central_map.y = (int)Canvas.GetTop(central_sensor1);

            right_map = new Position();

            RotateTransform t_rmap = (RotateTransform)right_sensor1.RenderTransform;

            right_map.angle = t_rmap.Angle * Math.PI / 180;

            right_map.x = (int)(Canvas.GetLeft(right_sensor1));
            right_map.y = (int)(Canvas.GetTop(right_sensor1));

            wallback_map = new Position();

            RotateTransform t_wbmap = (RotateTransform)wallback_sensor1.RenderTransform;

            wallback_map.angle = t_wbmap.Angle * Math.PI / 180;

            wallback_map.x = (int)Canvas.GetLeft(wallback_sensor1);
            wallback_map.y = (int)Canvas.GetTop(wallback_sensor1);

            wall_map = new Position();

            RotateTransform t_wmap = (RotateTransform)wall_sensor1.RenderTransform;

            wall_map.angle = t_wmap.Angle * Math.PI / 180;


            wall_map.x = (int)(Canvas.GetLeft(wall_sensor1));
            wall_map.y = (int)(Canvas.GetTop(wall_sensor1) + wall_sensor1.ActualWidth / 2);

            //MACRO WIDTH = 148
            mmperpixel_map = 148 / structure1.ActualWidth;
        }

        public void Init()
        {
            robot.SizeChanged += new SizeChangedEventHandler(macro_SizeChanged);
            mmperpixel_robotmap = 2.0;

            central_line = new Line();
            robot.Children.Add(central_line);

            right_line = new Line();
            robot.Children.Add(right_line);

            wall_line = new Line();
            robot.Children.Add(wall_line);

            wallback_line = new Line();
            robot.Children.Add(wallback_line);

            Canvas.SetTop(macro, map.ActualHeight / 2 - macro.ActualHeight / 2);
            Canvas.SetLeft(macro, map.ActualWidth / 2 - macro.ActualWidth / 2);

            StartupSensors();
            StartupMap();

            PosX = new List<double>();
            PosY = new List<double>();

            VelX = new List<double>();
            VelY = new List<double>();

            AccelX = new List<double>();
            AccelY = new List<double>();

            timePX = new List<double>();
            timeVX = new List<double>();
            timePY = new List<double>();
            timeVY = new List<double>();
            timeAX = new List<double>();
            timeAY = new List<double>();

            updateChart = new System.Threading.Timer(new System.Threading.TimerCallback(_updateChart), new object(), 1000, 1000);
        }

        private void StartupMap()
        {
            StartingPositionMap = new Position();
            StartingPositionMap.x = (int)Canvas.GetLeft(macro);
            StartingPositionMap.y = (int)Canvas.GetTop(macro);
            StartingPositionMap.angle = 0;

            actualPositionMap = new Position();
            actualPositionMap.x = StartingPositionMap.x;
            actualPositionMap.y = StartingPositionMap.y;
            actualPositionMap.angle = StartingPositionMap.angle;

            actualPosition = new Position();
        }

        void macro_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Init();
        }


    }

    public class Position
    {
        private double _x;
        private double _y;
        private double _angle;

        public double x { get { return _x; } set { _x = value; } }
        public double y { get { return _y; } set { _y = value; } }
        public double angle { get { return _angle; } set { _angle = value; } }
    }
}
