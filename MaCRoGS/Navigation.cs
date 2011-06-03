using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MaCRoGS
{
        public partial class MainWindow : Window
        {
            private void _SetHeading(double heading)
            {
                heading = heading * 180 / Math.PI;
                macro.RenderTransform = new RotateTransform(heading);
            }
        }
    
}
