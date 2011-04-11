using System;
using Microsoft.SPOT;
using MaCRo.Drivers;
using MaCRo.Config;

namespace MaCRo.Core
{
    public enum Sensor
    {
        Central, //S1
        wall_back,//L1
        Right,//L2
        Wall//S2
    }

    public class SensorManager
    {
        private DistanceDetector central;
        private DistanceDetector wall;
        private DistanceDetector wall_back;
        private DistanceDetector right;

        public SensorManager()
        {
            central = new DistanceDetector(PortMap.infraredL1, DistanceDetector.SharpSensorType.GP2Y0A21YK);
            wall = new DistanceDetector(PortMap.infraredS2, DistanceDetector.SharpSensorType.GP2D120);
            wall_back = new DistanceDetector(PortMap.infraredS1, DistanceDetector.SharpSensorType.GP2D120);
            right = new DistanceDetector(PortMap.infraredL2, DistanceDetector.SharpSensorType.GP2Y0A21YK);
        }

        public float getDistance(Sensor type)
        {
            float value = 0;

            switch (type)
            {
                case Sensor.Central:
                    for (int i = 0; i < 5; i++)
                    {
                        value += central.GetDistance_cm();
                    }
                    break;
                case Sensor.Wall:
                    for (int i = 0; i < 5; i++)
                    {
                        value += wall.GetDistance_cm();
                    }
                    break;
                case Sensor.wall_back: for (int i = 0; i < 5; i++)
                    {
                        value += wall_back.GetDistance_cm();
                    }
                    break;
                case Sensor.Right: for (int i = 0; i < 5; i++)
                    {
                        value += right.GetDistance_cm();
                    }
                    break;
                default:
                    return 0;

            }

            return value / 5;
        }


    }
}
