using System;
using Microsoft.SPOT;

namespace MaCRo.Tools
{
    public enum Mode
    {
        SearchingForWall,
        FollowWall,
        Manual
    }

    public enum Axis
    {
        X, Y, Z
    }

    public enum Movement
    {
        left,
        right,
        forward,
        backward,
        stop
    }

    public enum Message
    {
        PositionX,
        PositionY,
        Angle,
        VelocityX,
        VelocityY,
        Time,
        Pitch,
        Roll,
        Yaw,
        MAGHeading,
        SensorS1,
        SensorS2,
        SensorL1,
        SensorL2,
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
        IMUTempZ,
        Info,
        Debug,
        Error,
        MapUpdate1,
        MapUpdate2,
        MapUpdate3,
        MapUpdate4,
        PosUpdate,
        MapSize,
        Voltage,
        Current,
        Capacity,
        Estimation,
        MagX,
        MagY,
        MagZ
    }

    public class Position
    {
        public double x { get; set; }
        public double y { get; set; }
        private double _angle;

        public double angle { get { return _angle; } set { _angle = (value % (2 * System.Math.PI)); } }

        public Position()
        {
            x = 0;
            y = 0;
            _angle = 0;
        }
    }
}
