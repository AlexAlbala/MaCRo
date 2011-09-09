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
using System.Windows.Shapes;
using MaCRoGS.Communications;

namespace MaCRoGS
{
    /// <summary>
    /// Interaction logic for Manual.xaml
    /// </summary>
    public partial class Manual : Window
    {
        private Coder coder;
        private bool activated;
        public Manual(Coder coder)
        {
            this.coder = coder;
            InitializeComponent();
            activated = false;

            short value = (short)(Speed.Value * 10);
            SpeedBox.Text = value.ToString();

            value = (short)(Turning_Speed.Value * 10);
            TurningSpeedBox.Text = value.ToString();

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

        }

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
    }
}
