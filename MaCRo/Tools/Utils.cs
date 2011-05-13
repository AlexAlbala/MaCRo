using System;
using Microsoft.SPOT;

namespace MaCRo.Tools
{
    public enum Mode
    {
        SearchingForWall,
        FollowWall
    }

    public enum Axis
    {
        X,Y,Z
    }

    public enum Message
    {
        PositionX,
        PositionY,
        VelocityX,
        VelocityY,
        Angle,
        Time,
        SensorS1,
        SensorS2,
        SensorL1,
        SensorL2,
        Mode,
        IMUAccX,
        IMUAccY,
        IMUAccZ,
        IMUGyrX,
        IMUGyrY,
        IMUGyrZ,
        IMUMagX,
        IMUMagY,
        IMUMagZ,
        IMUTempX,
        IMUTempY,
        IMUTempZ
    }

    public class Position
    {
        private double _x;
        private double _y;
        private double _angle;

        public Position() 
        {
            _x = 0; 
            _y = 0; 
            _angle = 0; 
        }

        public double x { get { return _x; } set { _x = value; } }
        public double y { get { return _y; } set { _y = value; } }
        public double angle { get { return _angle; } set { _angle = value; } }
    }
}
