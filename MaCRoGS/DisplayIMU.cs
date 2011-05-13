using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MaCRoGS
{
    public partial class MainWindow : Window
    {
        public void SetAccX(double value)
        {
            UpdaterD d = new UpdaterD(_SetAccX);
            this.Dispatcher.Invoke(d, value);
        }

        private void _SetAccX(double value)
        {
            double val = value;
            AccX.Content = "X Acc: " + val.ToString();

            timeAX.Add(actualTime);
            AccelX.Add(value);
        }

        public void SetAccY(double value)
        {
            UpdaterD d = new UpdaterD(_SetAccY);
            this.Dispatcher.Invoke(d, value);
        }

        private void _SetAccY(double value)
        {
            double val = value;
            AccY.Content = "Y Acc: " + val.ToString();

            timeAY.Add(actualTime);
            AccelY.Add(value);
        }

        public void SetAccZ(double value)
        {
            UpdaterD d = new UpdaterD(_SetAccZ);
            this.Dispatcher.Invoke(d, value);
        }

        private void _SetAccZ(double value)
        {
            double val = value;
            AccZ.Content = "Z Acc: " + val.ToString();
        }

        public void SetMagX(double value)
        {
            UpdaterD d = new UpdaterD(_SetMagX);
            this.Dispatcher.Invoke(d, value);
        }

        private void _SetMagX(double value)
        {
            double val = value;
            MagX.Content = "X Mag: " + val.ToString();
        }

        public void SetMagY(double value)
        {
            UpdaterD d = new UpdaterD(_SetMagY);
            this.Dispatcher.Invoke(d, value);
        }

        private void _SetMagY(double value)
        {
            double val = value;
            MagY.Content = "Y Mag: " + val.ToString();
        }

        public void SetMagZ(double value)
        {
            UpdaterD d = new UpdaterD(_SetMagZ);
            this.Dispatcher.Invoke(d, value);
        }

        private void _SetMagZ(double value)
        {
            double val = value;
            MagZ.Content = "Z Mag: " + val.ToString();
        }

        public void SetTempX(double value)
        {
            UpdaterD d = new UpdaterD(_SetTempX);
            this.Dispatcher.Invoke(d, value);
        }

        private void _SetTempX(double value)
        {
            double val = value;
            TempX.Content = "X Temp: " + val.ToString();
        }

        public void SetTempY(double value)
        {
            UpdaterD d = new UpdaterD(_SetTempY);
            this.Dispatcher.Invoke(d, value);
        }

        private void _SetTempY(double value)
        {
            double val = value;
            TempY.Content = "Y Temp: " + val.ToString();
        }

        public void SetTempZ(double value)
        {
            UpdaterD d = new UpdaterD(_SetTempZ);
            this.Dispatcher.Invoke(d, value);
        }

        private void _SetTempZ(double value)
        {
            double val = value;
            TempZ.Content = "Z Temp: " + val.ToString();
        }

        public void SetGyroX(double value)
        {
            UpdaterD d = new UpdaterD(_SetGyroX);
            this.Dispatcher.Invoke(d, value);
        }

        private void _SetGyroX(double value)
        {
            double val = value;
            GyroX.Content = "X Gyro: " + val.ToString();
        }

        public void SetGyroY(double value)
        {
            UpdaterD d = new UpdaterD(_SetGyroY);
            this.Dispatcher.Invoke(d, value);
        }

        private void _SetGyroY(double value)
        {
            double val = value;
            GyroY.Content = "Y Gyro: " + val.ToString();
        }

        public void SetGyroZ(double value)
        {
            UpdaterD d = new UpdaterD(_SetGyroZ);
            this.Dispatcher.Invoke(d, value);
        }

        private void _SetGyroZ(double value)
        {
            double val = value;
            GyroZ.Content = "Z Gyro: " + val.ToString();
        }
    }
}
