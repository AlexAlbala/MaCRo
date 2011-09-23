using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace MaCRoGS
{
    public partial class MainWindow : Window
    {
        public void setCurrent(double mAmps)
        {
            UpdaterD d = new UpdaterD(_setCurrent);
            this.Dispatcher.Invoke(d, Math.Round(mAmps,3));
        }

        private void _setCurrent(double mAmps)
        {
            lbl_current.Content = "Current: " + mAmps.ToString() + " mA";
        }

        public void setVoltage(double volts)
        {
            UpdaterD d = new UpdaterD(_setVoltage);
            this.Dispatcher.Invoke(d, Math.Round(volts,3));
        }

        private void _setVoltage(double volts)
        {
            lbl_voltage.Content = "Voltage: " + volts.ToString() + " V";
        }

        public void setRVoltage(ushort perCent)
        {
            UpdaterUS d = new UpdaterUS(_setRVoltage);
            this.Dispatcher.Invoke(d, perCent);
        }

        private void _setRVoltage(ushort perCent)
        {
            lbl_perCent.Content = "Capacity: " + perCent.ToString() + " %";
            batteryBar.Value = perCent;

            if (perCent > 70)
                batteryBar.Foreground = Brushes.DarkGreen;
            else if (perCent > 50)
                batteryBar.Foreground = Brushes.GreenYellow;
            else if (perCent > 25)
                batteryBar.Foreground = Brushes.Yellow;
            else if (perCent > 15)
                batteryBar.Foreground = Brushes.DarkOrange;
            else if (perCent > 5)
                batteryBar.Foreground = Brushes.Red;
            
        }

        public void setEstimation(ushort minutes)
        {
            UpdaterUS d = new UpdaterUS(_setEstimation);
            this.Dispatcher.Invoke(d, minutes);
        }

        private void _setEstimation(ushort minutes)
        {
            lbl_estimation.Content = "Estimation: " + ((int)(minutes / 60)).ToString() + " h " + (minutes % 60).ToString() + " m";

        }
    }
}
