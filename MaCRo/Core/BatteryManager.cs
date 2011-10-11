using System;
using Microsoft.SPOT;
using MaCRo.Drivers;
using MaCRo.Tools;

namespace MaCRo.Core
{
    class BatteryManager
    {
        private VoltageCurrentSensor vcs;

        public bool lowBattery { get { return getBatteryCapacity() < 5 ? true : false; } }

        public BatteryManager()
        {
            vcs = new VoltageCurrentSensor(11.6f, 9.0f, (ushort)4000);
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

        public ushort getBatteryEstimation_minutes()
        {
            return vcs.getEstimation();
        }
    }
}
