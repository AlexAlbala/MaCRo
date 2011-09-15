using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MaCRoGS
{
    public partial class MainWindow : Window
    {
        public void setCurrent(double mAmps)
        {
            UpdaterD d = new UpdaterD(_setCurrent);
            this.Dispatcher.Invoke(d, mAmps);
        }

        private void _setCurrent(double mAmps)
        {
            lbl_current.Content = "Current: " + mAmps.ToString() + " mA";
        }

        public void setVoltage(double volts)
        {
            UpdaterD d = new UpdaterD(_setVoltage);
            this.Dispatcher.Invoke(d, volts);
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
            
        }
    }
}
