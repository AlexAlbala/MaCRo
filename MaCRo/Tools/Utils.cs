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
        MoveForward,
        MoveBackward,
        TurnRight,
        TurnLeft,
        Object,
        SensorS1,
        SensorS2,
        SensorL1,
        SensorL2,
        Stop,
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
        private int _x;
        private int _y;
        public int x { get { return _x; } set { _x = value; } }
        public int y { get { return _y; } set { _y = value; } }
    }
}
