using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MaCRoGS.Communications
{
    public enum Message
    {
        Forward,
        Backward,
        TurnLeft,
        TurnRight,
        Stop,
        ToManual,
        StopManual,
        Speed,
        TurningSpeed,
        UpdatePosition
    }
}
