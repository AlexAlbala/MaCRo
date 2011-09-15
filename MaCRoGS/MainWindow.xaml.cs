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
using MaCRoGS.SLAM;
using System.Threading;

namespace MaCRoGS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Coder coder;
        private delegate void Updater(short value);
        private delegate void UpdaterString(string message);
        private delegate void UpdaterInt(int value);
        private delegate void UpdaterD(double value);
        private delegate void UpdaterUSA(ushort[] value, short part);
        private delegate void UpdaterUSAF(ushort[] value);
        private delegate void UpdaterUS(ushort value);

        private short updates = 0;

        private Timer scanTimer;
        private Timer updateMap;
        private Timer positionHistoryUpdate;
        private Timer updatePosition;

        private bool activated;

        public MainWindow()
        {
            try
            {
                coder = new Coder();
                coder.Start(this);
            }
            catch (Exception e)
            {
                MessageBoxResult mbres = MessageBox.Show("Could not initialize the coder. Original message: " + e.Message + " Do you want to continue anyway?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (mbres == MessageBoxResult.No)
                {
                    Application.Current.Shutdown();
                }
            }
            InitializeComponent();
        }

        public void StartupSensors()
        {
            //CENTRAL SENSOR *******************************
            central_robotmap = new Position();
            central_robotmap.angle = 0;
            central_robotmap.x = (int)(Canvas.GetLeft(central_sensor) + central_sensor.ActualWidth / 2);
            central_robotmap.y = (int)Canvas.GetTop(central_sensor);

            //RIGHT SENSOR *********************************
            right_robotmap = new Position();
            RotateTransform t_rrmap = (RotateTransform)right_sensor.RenderTransform;
            right_robotmap.angle = t_rrmap.Angle * Math.PI / 180;
            right_robotmap.x = (int)(Canvas.GetLeft(right_sensor));
            right_robotmap.y = (int)(Canvas.GetTop(right_sensor));

            //WALL BACK SENSOR *****************************
            wallback_robotmap = new Position();
            RotateTransform t_wbrmap = (RotateTransform)wallback_sensor.RenderTransform;
            wallback_robotmap.angle = t_wbrmap.Angle * Math.PI / 180;
            wallback_robotmap.x = (int)Canvas.GetLeft(wallback_sensor);
            wallback_robotmap.y = (int)Canvas.GetTop(wallback_sensor);

            //WALL SENSOR ***********************************
            wall_robotmap = new Position();
            RotateTransform t_wrmap = (RotateTransform)wall_sensor.RenderTransform;
            wall_robotmap.angle = t_wrmap.Angle * Math.PI / 180;
            wall_robotmap.x = (int)(Canvas.GetLeft(wall_sensor));
            wall_robotmap.y = (int)(Canvas.GetTop(wall_sensor) + wall_sensor.ActualWidth / 2);


        }

        public void Init()
        {
            mmperpixel_robotmap = 2.0;

            central_line = new Line();
            robot.Children.Add(central_line);

            right_line = new Line();
            robot.Children.Add(right_line);

            wall_line = new Line();
            robot.Children.Add(wall_line);

            wallback_line = new Line();
            robot.Children.Add(wallback_line);

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

            updateChart = new Timer(new TimerCallback(_updateChart), new object(), 1000, 1000);

            /************************** TIMERS ***************************************/
            scanTimer = new Timer(new TimerCallback(Scan), new object(), 4000, 100);
            updateMap = new Timer(new TimerCallback(mapUpdate), new object(), 4000, 3000);
            updatePosition = new Timer(new TimerCallback(MonteCarlo_UpdatePosition), new object(), 30000, 3000);
            positionHistoryUpdate = new Timer(new TimerCallback(UpdatePositionHistory), new object(), 0, 250);
            /************************** TIMERS ***************************************/

            activated = false;
            tinySLAM.Initialize();

            lbl_holeWidth.Content = "HOLE WIDTH: " + tinySLAM.HoleWidth() + " mm";
            lbl_mapScale.Content = "MAP SCALE: " + (1 / tinySLAM.MapScale()).ToString() + " mm/cell";
            lbl_mapSize.Content = "MAP SIZE: " + tinySLAM.MapSize() + " cells";
            lbl_scanSize.Content = "SCAN SIZE: " + tinySLAM.ScanSize() + " points";

            lbl_perCent.Content = "Capacity:";
            lbl_voltage.Content = "Voltage:";
            lbl_current.Content = "Current:";

            this.updateIterationsLabel(0);
            this.updateMapUpdatesLabel(0);
            log.SelectAll();
            log.Selection.Text = "";
        }

        private void StartupMap()
        {
            actualPosition = new Position();
            positionHistory = new List<Position>();
        }

        private void UpdatePositionHistory(Object state)
        {
            Position p = new Position();
            p.x = actualPosition.x;
            p.y = actualPosition.y;
            p.angle = actualPosition.angle;
            positionHistory.Add(p);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Init();
        }

        private void updateIterationsLabel(int num)
        {
            lbl_iterations.Content = "ITERATIONS: " + num.ToString();
        }

        private void updateMapUpdatesLabel(int num)
        {
            lbl_mapUpdates.Content = "UPDATES: " + num.ToString();
        }

        public void mapUpdate(object state)
        {
            UpdaterInt d = new UpdaterInt(updateMapUpdatesLabel);
            this.Dispatcher.Invoke(d, ++updates);

            this.UpdateMap(tinySLAM.Map().map, (short)-1);
        }

        void MonteCarlo_UpdatePosition(Object state)
        {
            ts_position_t position = new ts_position_t();

            position.theta = (float)(actualPosition.angle * 180 / Math.PI);
            position.x = 1000 * actualPosition.x + SLAMAlgorithm.TS_MAP_SIZE / (SLAMAlgorithm.TS_MAP_SCALE * 2);
            position.y = 1000 * actualPosition.y + SLAMAlgorithm.TS_MAP_SIZE / (SLAMAlgorithm.TS_MAP_SCALE * 2);

            ts_scan_t scan = this.doScan();

            int quality;
            ts_position_t newPos = tinySLAM.MonteCarlo_UpdatePosition(scan, position, 0, 1, 10000, 50, out quality);


            Position p = new Position();
            p.x = (newPos.x - SLAMAlgorithm.TS_MAP_SIZE / (SLAMAlgorithm.TS_MAP_SCALE * 2)) * 1e-3;
            p.y = (newPos.y - SLAMAlgorithm.TS_MAP_SIZE / (SLAMAlgorithm.TS_MAP_SCALE * 2)) * 1e-3;
            p.angle = newPos.theta * Math.PI / 180;

            coder.Send(Message.UpdatePosition, p);
        }

        private ts_scan_t doScan()
        {
            ts_scan_t scan = new ts_scan_t();

            int j = 0;
            int angle = 0;
            //CENTRAL SENSOR:
            if (lastCentral != -1)
            {
                angle -= 10;
                for (int i = 0; i < 20; angle++, i++, j++)
                {
                    double angleRad = (float)(angle * Math.PI / 180);
                    double c = Math.Cos(angleRad);
                    double s = Math.Sin(angleRad);


                    scan.x[j] = (float)(lastCentral * s);
                    scan.y[j] = (float)(lastCentral * c + 230 / 2) * (-1);
                    scan.value[j] = SLAMAlgorithm.TS_OBSTACLE;
                }
            }


            //WALL SENSOR
            if (lastWall != -1)
            {
                angle = 0;
                angle -= 10;
                for (int i = 0; i < 20; angle++, i++, j++)
                {
                    double angleRad = (angle * Math.PI / 180);
                    double c = Math.Cos(angleRad);
                    double s = Math.Sin(angleRad);


                    scan.x[j] = (float)(lastWall * c + 185 / 2) * (-1);
                    scan.y[j] = (float)(230 / 2 + lastWall * s) * (-1);
                    scan.value[j] = SLAMAlgorithm.TS_OBSTACLE;
                }
            }

            //WALLBACK SENSOR
            if (lastWallBack != -1)
            {
                angle = 0;
                angle -= 10;
                for (int i = 0; i < 20; angle++, i++, j++)
                {
                    double angleRad = (angle * Math.PI / 180);
                    double c = Math.Cos(angleRad);
                    double s = Math.Sin(angleRad);


                    scan.x[j] = (float)(lastWallBack * c + 185 / 2) * (-1);
                    scan.y[j] = (float)(lastWallBack * s - 230 / 2) * (-1);
                    scan.value[j] = SLAMAlgorithm.TS_OBSTACLE;
                }
            }

            //RIGHT SENSOR
            if (lastRight != -1)
            {
                angle = 0;
                angle -= 10;
                for (int i = 0; i < 20; angle++, i++, j++)
                {
                    double angleRad = (angle * Math.PI / 180);
                    double rad45 = (45 * Math.PI / 180);
                    double c = Math.Cos(angleRad + rad45);
                    double s = Math.Sin(angleRad + rad45);


                    scan.x[j] = (float)(lastRight * c + 185 / 2);
                    scan.y[j] = (float)(lastRight * s + 230 / 2) * (-1);
                    scan.value[j] = SLAMAlgorithm.TS_OBSTACLE;
                }
            }

            scan.nb_points = (ushort)j;

            return scan;
        }

        public void Scan(object state)
        {
            UpdaterInt d = new UpdaterInt(updateIterationsLabel);
            this.Dispatcher.Invoke(d, tinySLAM.NumUpdates());

            ts_scan_t scan = this.doScan();

            ts_position_t position = new ts_position_t();

            position.theta = (float)(actualPosition.angle * 180 / Math.PI);
            position.x = 1000 * actualPosition.x + SLAMAlgorithm.TS_MAP_SIZE / (SLAMAlgorithm.TS_MAP_SCALE * 2);
            position.y = 1000 * actualPosition.y + SLAMAlgorithm.TS_MAP_SIZE / (SLAMAlgorithm.TS_MAP_SCALE * 2);
            tinySLAM.NewScan(scan, position);
        }

        #region Handlers
        private void for_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // TODO: Add event handler implementation here.
            coder.Send(Message.Forward, (short)25);
        }

        private void for_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // TODO: Add event handler implementation here.
            coder.Send(Message.Stop, null);
            forw.IsChecked = true;
        }

        private void left_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // TODO: Add event handler implementation here.
            coder.Send(Message.TurnLeft, null);
        }

        private void left_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // TODO: Add event handler implementation here.
            coder.Send(Message.Stop, null);
            left.IsChecked = true;
        }

        private void back_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // TODO: Add event handler implementation here.
            coder.Send(Message.Backward, (short)25);
        }

        private void back_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // TODO: Add event handler implementation here.
            coder.Send(Message.Stop, null);
            back.IsChecked = true;
        }

        private void right_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // TODO: Add event handler implementation here.
            coder.Send(Message.TurnRight, null);
        }

        private void right_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // TODO: Add event handler implementation here.
            coder.Send(Message.Stop, null);
            right.IsChecked = true;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // TODO: Add event handler implementation here.
            if (activated)
            {
                coder.Send(Message.StopManual, null);
                OnOff.Content = "On";

                LEDImageOFF.Visibility = System.Windows.Visibility.Visible;
                LEDImageON.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                coder.Send(Message.ToManual, null);
                OnOff.Content = "Off";

                LEDImageOFF.Visibility = System.Windows.Visibility.Hidden;
                LEDImageON.Visibility = System.Windows.Visibility.Visible;
            }

            activated = !activated;
        }

        private void Speed_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            // TODO: Add event handler implementation here.
            short value = (short)(e.NewValue * 10);
            if (SpeedBox != null)
            {
                SpeedBox.Text = value.ToString();
            }

            coder.Send(Message.Speed, value);

        }

        private void Turning_Speed_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            // TODO: Add event handler implementation here.

            short value = (short)(e.NewValue * 10);
            if (TurningSpeedBox != null)
            {
                TurningSpeedBox.Text = value.ToString();
            }

            coder.Send(Message.TurningSpeed, value);
        }

        private void SpeedBox_Initialized(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            short value = (short)(Speed.Value * 10);
            SpeedBox.Text = value.ToString();
        }

        private void TurningSpeedBox_Initialized(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
            short value = (short)(Turning_Speed.Value * 10);
            TurningSpeedBox.Text = value.ToString();
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // TODO: Add event handler implementation here.
            coder.Send(Message.Stop, null);

            switch (e.Key)
            {
                case Key.Left:
                    left.IsChecked = false;
                    break;
                case Key.Up:
                    forw.IsChecked = false;
                    break;
                case Key.Right:
                    right.IsChecked = false;
                    break;
                case Key.Down:
                    back.IsChecked = false;
                    break;
                default:
                    break;
            }

            coder.Send(Message.Stop, null);
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // TODO: Add event handler implementation here.
            switch (e.Key)
            {
                case Key.Left:
                    coder.Send(Message.TurnLeft, null);
                    left.IsChecked = true;
                    break;
                case Key.Up:
                    coder.Send(Message.Forward, (short)25);
                    forw.IsChecked = true;
                    break;
                case Key.Right:
                    coder.Send(Message.TurnRight, null);
                    right.IsChecked = true;
                    break;
                case Key.Down:
                    coder.Send(Message.Backward, (short)25);
                    back.IsChecked = true;
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Log
        private void updateLog(string text)
        {
            FlowDocument mcFlowDoc = new FlowDocument();
            mcFlowDoc = log.Document;
            Paragraph pr = new Paragraph();
            pr.Inlines.Add(DateTime.Now.ToString() + " : " + text);
            mcFlowDoc.Blocks.Add(pr);
            log.Document = mcFlowDoc;

            log.ScrollToEnd();
        }

        public void info(string text)
        {
            string message = "INFO-" + text;
            UpdaterString d = new UpdaterString(updateLog);
            this.Dispatcher.Invoke(d, message);
        }

        public void debug(string text)
        {
            string message = "DEBUG-" + text;
            UpdaterString d = new UpdaterString(updateLog);
            this.Dispatcher.Invoke(d, message);
        }

        public void error(string text)
        {
            string message = "ERROR-" + text;
            UpdaterString d = new UpdaterString(updateLog);
            this.Dispatcher.Invoke(d, message);
        }

        #endregion

    }

    public class Position
    {
        private double _x;
        private double _y;
        private double _angle;

        public double x { get { return _x; } set { _x = value; } }
        public double y { get { return _y; } set { _y = value; } }
        public double angle { get { return _angle; } set { _angle = (value % (2 * Math.PI)); } }
    }
}
