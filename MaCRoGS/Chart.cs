using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Windows.Media;
using System.Windows.Controls;
using System.Threading;

namespace MaCRoGS
{
    public partial class MainWindow : Window
    {
        private List<double> PosX;
        private List<double> PosY;

        private List<double> VelX;
        private List<double> VelY;

        private List<double> AccelX;
        private List<double> AccelY;

        private List<double> timePX;
        private List<double> timePY;

        private List<double> timeVX;
        private List<double> timeVY;

        private List<double> timeAX;
        private List<double> timeAY;

        private LineGraph PosxLine;
        private LineGraph PosyLine;

        private LineGraph VelxLine;
        private LineGraph VelyLine;

        private LineGraph AccxLine;
        private LineGraph AccyLine;

        private delegate LineGraph ChartUpdater(LineGraph line, double[] x, double[] y, Color color, string description);

        private Timer updateChart;

        private double actualTime;

        public void SetPositionX(double distance)
        {
            UpdaterD updater = new UpdaterD(_SetPositionX);
            this.Dispatcher.Invoke(updater, distance);

        }

        void _updateChart(object state)
        {
            if (PosX.Count > 0 && timePX.Count > 0)
            {
                UpdateLine(PosxLine, timePX.ToArray(), PosX.ToArray(), Colors.Black, "Position X", out PosxLine);
            }
            if (PosY.Count > 0 && timePY.Count > 0)
            {
                UpdateLine(PosyLine, timePY.ToArray(), PosY.ToArray(), Colors.Blue, "Position Y", out PosyLine);
            }
            if (VelX.Count > 0 && timeVX.Count > 0)
            {
                UpdateLine(VelxLine, timeVX.ToArray(), VelX.ToArray(), Colors.Red, "Velocity X", out VelxLine);
            }
            if (VelY.Count > 0 && timeVY.Count > 0)
            {
                UpdateLine(VelyLine, timeVY.ToArray(), VelY.ToArray(), Colors.DarkGreen, "Velocity Y", out VelyLine);
            }
            if (AccelX.Count > 0 && timeAX.Count > 0)
            {
                UpdateLine(AccxLine, timeAX.ToArray(), AccelX.ToArray(), Colors.DarkGray, "Acceleration X", out AccxLine);
            }
            if (AccelY.Count > 0 && timeAY.Count > 0)
            {
                UpdateLine(AccyLine, timeAY.ToArray(), AccelY.ToArray(), Colors.Brown, "Acceleration Y", out AccyLine);
            }
        }

        private void _SetPositionX(double posX)
        {
            xPos.Content = "Position in X axis: " + posX.ToString() + " meters";
            actualPosition.x = posX;

            actualPositionMap.x = StartingPositionMap.x + (1000 * actualPosition.x / this.mmperpixel_map);

            Canvas.SetLeft(macro, actualPositionMap.x - (Canvas.GetLeft(structure1) + structure1.ActualWidth / 2) * Math.Cos(actualPositionMap.angle) + (Canvas.GetTop(structure1) + structure1.ActualHeight * Math.Sin(actualPositionMap.angle)) / 2);

            timePX.Add(actualTime);
            PosX.Add(posX);
        }

        public void SetPositionY(double distance)
        {
            UpdaterD updater = new UpdaterD(_SetPositionY);
            this.Dispatcher.Invoke(updater, distance);
        }

        public void SetVelocityX(double distance)
        {
            UpdaterD updater = new UpdaterD(_SetVelocityX);
            this.Dispatcher.Invoke(updater, distance);

        }

        private void _SetVelocityX(double velX)
        {
            xVel.Content = "Velocity in X axis: " + velX.ToString() + " m/s";

            timeVX.Add(actualTime);
            VelX.Add(velX);

        }

        public void SetVelocityY(double distance)
        {
            UpdaterD updater = new UpdaterD(_SetVelocityY);
            this.Dispatcher.Invoke(updater, distance);

        }

        private void _SetVelocityY(double velY)
        {
            yVel.Content = "Velocity in Y axis: " + velY.ToString() + " m/s";

            timeVY.Add(actualTime);
            VelY.Add(velY);

        }

        private void UpdateLine(LineGraph line, double[] x, double[] y, Color color, string description, out LineGraph newLine)
        {
            ChartUpdater cup = new ChartUpdater(_UpdateLine);
            object _line = this.Dispatcher.Invoke(cup, line, x, y, color, description);
            newLine = _line as LineGraph;
        }

        private LineGraph _UpdateLine(LineGraph line, double[] x, double[] y, Color color, string description)
        {
            lock (chart)
            {
                if (line != null)
                {
                    chart.Children.Remove(line);
                }

                var xData = x.AsXDataSource();
                var yData = y.AsYDataSource();

                CompositeDataSource compositeDataSource = xData.Join(yData);
                // adding graph to plotter
                LineGraph newLine = chart.AddLineGraph(compositeDataSource, color, 2, description);

                // Force evertyhing plotted to be visible
                chart.FitToView();

                return newLine;

            }
        }

        private void _SetPositionY(double Y)
        {
            yPos.Content = "Position in Y axis: " + Y.ToString() + " meters";
            actualPosition.y = Y;

            actualPositionMap.y = StartingPositionMap.y + (1000 * actualPosition.y / this.mmperpixel_map);

            Canvas.SetTop(macro, actualPositionMap.y - (Canvas.GetTop(structure1) + structure1.ActualHeight / 2) * Math.Cos(actualPositionMap.angle) - (Canvas.GetLeft(structure1) + structure1.ActualWidth * Math.Sin(actualPositionMap.angle) / 2));

            macro.UpdateLayout();

            timePY.Add(actualTime);

            PosY.Add(Y);
        }

        public void SetPositionAngle(double distance)
        {
            UpdaterD updater = new UpdaterD(_SetPositionAngle);
            this.Dispatcher.Invoke(updater, distance);
        }

        private void _SetPositionAngle(double angle)
        {
            anglePos.Content = "Current angle: " + angle.ToString() + " rads";
            this._SetHeading(angle);
        }

        public void SetTime(double time)
        {
            UpdaterD updater = new UpdaterD(_SetTime);
            this.Dispatcher.Invoke(updater, time);
        }

        private void _SetTime(double time)
        {
            actualTime = time;
            labelTime.Content = "Current Time: " + time.ToString() + " seconds";
        }
    }
}
