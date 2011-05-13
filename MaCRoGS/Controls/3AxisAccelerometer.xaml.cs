using System;
using System.Collections.Generic;
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

namespace MaCRoGS
{
    /// <summary>
    /// Interaction logic for _AxisAccelerometer.xaml
    /// </summary>
    public partial class _AxisAccelerometer : UserControl
    {
        private Ellipse _ball;
        public _AxisAccelerometer()
        {
            this.InitializeComponent();

            foreach (UIElement _e in LayoutRoot.Children)
            {
                if (_e is Ellipse)
                {
                    Ellipse e = (Ellipse)_e;
                    if (e.Name == "ball")
                    {
                        _ball = e;
                    }
                }
            }
        }

        public void SetAccX(double value)
        {
            Canvas.SetLeft(_ball, value * 100);
        }

        public void SetAccY(double value)
        {
        }

        public void SetAccZ(double value)
        {
        }

    }
}