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
            double headingDEG = heading * 180 / Math.PI;

            //Save the central point of the element.
            RotateTransform t_macro = (RotateTransform)macro.RenderTransform;
            //IEnumerable<RotateTransform> transf = t_macro.Children.OfType<RotateTransform>();
            double angleRad = (t_macro.Angle) * Math.PI / 180;            

            double centralPointX = Canvas.GetLeft(macro) + Canvas.GetLeft(structure1) + structure1.ActualWidth / 2 - structure1.ActualHeight * Math.Sin(angleRad) / 2;
            double centralPointY = Canvas.GetTop(macro) + Canvas.GetTop(structure1) + structure1.ActualHeight / 2 + structure1.ActualWidth * Math.Cos(angleRad) / 2;

            macro.RenderTransform = new RotateTransform(headingDEG);

            //Correct the position
            Canvas.SetLeft(macro, centralPointX - Canvas.GetLeft(structure1) - structure1.ActualWidth / 2 + structure1.ActualHeight * Math.Sin(heading) / 2);
            Canvas.SetTop(macro, centralPointY - Canvas.GetTop(structure1) - structure1.ActualHeight / 2 - structure1.ActualWidth * Math.Cos(heading) / 2);

            macro.UpdateLayout();

            actualPositionMap.angle = heading;
        }
    }

}
