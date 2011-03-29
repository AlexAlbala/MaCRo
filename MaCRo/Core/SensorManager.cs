using System;
using Microsoft.SPOT;
using MaCRo.Drivers;
using MaCRo.Config;

namespace MaCRo.Core
{
    public enum Sensor
    {
        Central,
        Left,
        Right,
        Floor
    }

    public class SensorManager
    {
        private DistanceDetector dds1;
        private DistanceDetector dds2;
        private DistanceDetector ddl1;
        private DistanceDetector ddl2;

        public SensorManager()
        {
            dds1 = new DistanceDetector(PortMap.infraredS1_center, DistanceDetector.SharpSensorType.GP2D120);
            dds2 = new DistanceDetector(PortMap.infraredS2_down, DistanceDetector.SharpSensorType.GP2D120);
            ddl1 = new DistanceDetector(PortMap.infraredL1_left, DistanceDetector.SharpSensorType.GP2Y0A21YK);
            ddl2 = new DistanceDetector(PortMap.infraredL2_right, DistanceDetector.SharpSensorType.GP2Y0A21YK);
        }

        public float getDistance(Sensor type)
        {
            float value = 0;

            switch (type)
            {
                case Sensor.Central:
                    for (int i = 0; i < 5; i++)
                    {
                        value += dds1.GetDistance_cm();
                    }
                    break;
                case Sensor.Floor:
                    for (int i = 0; i < 5; i++)
                    {
                        value += dds2.GetDistance_cm();
                    }
                    break;
                case Sensor.Left: for (int i = 0; i < 5; i++)
                    {
                        value += ddl1.GetDistance_cm();
                    }
                    break;
                case Sensor.Right: for (int i = 0; i < 5; i++)
                    {
                        value += ddl2.GetDistance_cm();
                    }
                    break;
                default:
                    return 0;

            }

            return value / 5;
        }


    }
}
