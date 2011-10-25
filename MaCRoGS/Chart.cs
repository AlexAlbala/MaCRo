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

        private List<double> timePX;
        private List<double> timePY;       

        private LineGraph PosxLine;
        private LineGraph PosyLine;       

        private delegate LineGraph ChartUpdater(LineGraph line, double[] x, double[] y, Color color, string description);

        private Timer updateChart;

        private double actualTime;
        
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
        }        

        /*
        public void SetVelocityX(double distance)
        {
            UpdaterD updater = new UpdaterD(_SetVelocityX);
            this.Dispatcher.Invoke(updater, distance);

        }

        private void _SetTime(double time)
        {
            actualTime = time;
            labelTime.Content = "Current Time: " + time.ToString() + " seconds";
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
         */ 

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

        /*
        public void SetTime(double time)
        {
            UpdaterD updater = new UpdaterD(_SetTime);
            this.Dispatcher.Invoke(updater, time);
        }*/
    }
}
