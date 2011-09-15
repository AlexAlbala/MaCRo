using System;
using Microsoft.SPOT;
using MaCRo.Drivers;
using MaCRo.Tools;

namespace MaCRo.Core
{
    class BatteryManager
    {
        private VoltageCurrentSensor vcs;

        public BatteryManager()
        {
            vcs = new VoltageCurrentSensor(11.1f, 9.5f, (ushort)4000);
        }

        public double getBatteryVoltage()
        {
            return vcs.getActualVoltage();
        }

        public ushort getBatteryCapacity()
        {
            return vcs.getBatteryCapacity();
        }

        public double getBatteryCurrent()
        {
            return vcs.getActualCurrent();
        }
    }
}
