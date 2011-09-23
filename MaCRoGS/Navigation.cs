using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MaCRoGS.SLAM;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

namespace MaCRoGS
{
    public partial class MainWindow : Window
    {
        private ushort MapSizeValue = 0;
        private double Scale = 0;

        private Position actualPosition;
        private List<Position> positionHistory;
        System.Windows.Shapes.Path roverPath;

        private void _SetHeading(double heading)
        {
            double headingDEG = heading * 180 / Math.PI;
            actualPosition.angle = heading;
        }

        private void _MapSize(ushort NewSize)
        {
            this.MapSizeValue = (ushort)(NewSize);

            double WidthScale = map.ActualWidth / MapSizeValue;
            double HeightScale = map.ActualHeight / MapSizeValue;

            if (WidthScale < HeightScale)
                this.Scale = WidthScale;
            else
                this.Scale = HeightScale;
        }

        public void MapSize(ushort NewSize)
        {
            UpdaterUS d = new UpdaterUS(_MapSize);
            this.Dispatcher.Invoke(d, NewSize);
        }

        private void _UpdateMap(ushort[] NewMap, short part)
        {
            if (MapSizeValue == 0)
                return;

            int line, column = 0;
            int start = (part - 1) * (MapSizeValue * MapSizeValue) / 4;
            for (int i = start; i < start + NewMap.Length; i++)
            {
                line = i / MapSizeValue;
                column = i % MapSizeValue;

                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();

                rect.Width = Scale;
                rect.Height = Scale;

                byte value = (byte)(NewMap[i - start] * 255 / 65500);

                rect.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)(255 - value), (byte)(255 - value), (byte)(255 - value)));
                rect.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)(255 - value), (byte)(255 - value), (byte)(255 - value)));

                map.Children.Add(rect);

                Canvas.SetLeft(rect, column * Scale);
                Canvas.SetTop(rect, line * Scale);
            }

        }

        public void SetPositionX(double distance)
        {
            UpdaterD updater = new UpdaterD(_SetPositionX);
            this.Dispatcher.Invoke(updater, distance);

        }

        private void _SetPositionX(double posX)
        {
            xPos.Content = "Position in X axis: " + posX.ToString() + " meters";
            actualPosition.x = posX;

            timePX.Add(actualTime);
            PosX.Add(posX);
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

        public void SetPositionY(double distance)
        {
            UpdaterD updater = new UpdaterD(_SetPositionY);
            this.Dispatcher.Invoke(updater, distance);
        }

        private void _SetPositionY(double Y)
        {
            yPos.Content = "Position in Y axis: " + Y.ToString() + " meters";
            actualPosition.y = Y;

            timePY.Add(actualTime);

            PosY.Add(Y);
        }

        private void _UpdateFullMap(ushort[] NewMap)
        {
            if (roverPath == null)
            {
                roverPath = new System.Windows.Shapes.Path();
                map.Children.Add(roverPath);
                roverPath.Stroke = System.Windows.Media.Brushes.Red;

                DoubleCollection dashes = new DoubleCollection();
                dashes.Add(2);
                dashes.Add(2);
                roverPath.StrokeDashArray = dashes;
                roverPath.StrokeDashCap = PenLineCap.Flat;

                roverPath.Stroke.Freeze();
                roverPath.StrokeThickness = 2   ;
            }

            PathGeometry geometry = new PathGeometry();
            PathFigure trackFigure = new PathFigure();
            trackFigure.StartPoint = new System.Windows.Point(map.ActualWidth / 2, map.ActualHeight / 2);
            if ((bool)chkBox_path.IsChecked)
            {
                lock (positionHistory)
                {
                    for (int i = 0; i < positionHistory.Count; i++)
                    {
                        Position p = positionHistory[i];
                        double x = (p.x * 1000 * tinySLAM.MapScale() + SLAMAlgorithm.TS_MAP_SIZE / 2) * map.ActualWidth / tinySLAM.MapSize();
                        double y = (p.y * 1000 * tinySLAM.MapScale() + SLAMAlgorithm.TS_MAP_SIZE / 2) * map.ActualHeight / tinySLAM.MapSize();
                        System.Windows.Point _p = new System.Windows.Point(x, y);
                        trackFigure.Segments.Add(new LineSegment(_p, true));
                    }
                }

                geometry.Figures.Add(trackFigure);
                roverPath.Data = geometry;
            }
            else
            {
                if (roverPath != null)
                {
                    roverPath.Data = new PathGeometry();
                }
            }

            ushort _MapSizeValue = tinySLAM.MapSize();
            MapSize(_MapSizeValue);

            byte[] buffer = new byte[NewMap.Length * 3];
            for (int i = 0; i < NewMap.Length; i++)
            {
                if (NewMap[i] == (ushort)0)
                {
                    buffer[3 * i] = 255;
                    buffer[3 * i + 1] = 0;
                    buffer[3 * i + 2] = 0;
                }
                else
                {
                    byte value = (byte)(NewMap[i] * 255 / 65500);
                    buffer[3 * i] = value;
                    buffer[3 * i + 1] = value;
                    buffer[3 * i + 2] = value;
                }
            }

            Bitmap bmp = new Bitmap((int)_MapSizeValue, (int)_MapSizeValue, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
            Marshal.Copy(buffer, 0, bmpData.Scan0, buffer.Length);


            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();

            mapImage.Source = bi;
        }

        public void UpdateMap(ushort[] map, short part)
        {
            if (part == -1)
            {
                UpdaterUSAF d = new UpdaterUSAF(_UpdateFullMap);
                this.Dispatcher.Invoke(d, map);
            }
            else
            {
                UpdaterUSA d = new UpdaterUSA(_UpdateMap);
                this.Dispatcher.Invoke(d, map, part);
            }

        }
    }

}
