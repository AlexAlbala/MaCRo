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
            
        }      

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
        	// TODO: Add event handler implementation here.
            switch (e.Key)
            {
                case Key.Left:
                    coder.Send(Message.TurnLeft, null);
                    break;
                case Key.Up:
                    coder.Send(Message.Forward, (short)25);
                    break;
                case Key.Right:
                    coder.Send(Message.TurnRight, null);
                    break;
                case Key.Down:
                    coder.Send(Message.Backward, (short)25);
                    break;
                default:
                    break;
            }
          
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
        	// TODO: Add event handler implementation here.
            coder.Send(Message.Stop, null);
            
        }

        private void for_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	// TODO: Add event handler implementation here.
			coder.Send(Message.Forward,(short)25);
        }

        private void for_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	// TODO: Add event handler implementation here.
			coder.Send(Message.Stop, null);
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
        }

        private void back_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	// TODO: Add event handler implementation here.
			coder.Send(Message.Backward,(short)25);
        }

        private void back_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	// TODO: Add event handler implementation here.
			coder.Send(Message.Stop, null);
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
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
			if(activated)
			{
				coder.Send(Message.StopManual,null);
				OnOff.Content = "On";

                LEDImageOFF.Visibility = System.Windows.Visibility.Visible;
                LEDImageON.Visibility = System.Windows.Visibility.Hidden;
             
			}
			else
			{
				coder.Send(Message.ToManual,null);
				OnOff.Content = "Off";

                LEDImageOFF.Visibility = System.Windows.Visibility.Hidden;
                LEDImageON.Visibility = System.Windows.Visibility.Visible;

			}
			
			activated = !activated;
        }
    }
}
