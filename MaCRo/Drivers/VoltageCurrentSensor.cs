using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.Hardware;
using GHIElectronics.NETMF.FEZ;
using MaCRo.Config;

namespace MaCRo.Drivers
{
    class VoltageCurrentSensor
    {
        private AnalogIn current;
        private AnalogIn voltage;

        private float maxVoltage;
        private float minVoltage;
        private ushort mAh;

        private double ampScale;//Amp(in) / V(out)
        private double voltScale;//V(in) / V(out)

        public VoltageCurrentSensor(float maxVoltage, float minVoltage, ushort mAh)
        {
            this.mAh = mAh;
            this.maxVoltage = maxVoltage;
            this.minVoltage = minVoltage;

            current = new AnalogIn((AnalogIn.Pin)PortMap.current);
            voltage = new AnalogIn((AnalogIn.Pin)PortMap.voltage);

            voltScale = 1 / 63.69e-3;
            ampScale = 1 / 36.60e-3;

            voltage.SetLinearScale(0, 3300);
            current.SetLinearScale(0, 3300);
        }

        public ushort getBatteryCapacity()
        {
            double actualVoltage = this.getActualVoltage();
            if (actualVoltage < minVoltage)
                return 0;
            else if (actualVoltage > maxVoltage)
                return 100;
            else
                return (ushort)(100 * (actualVoltage - minVoltage) / (maxVoltage - minVoltage));
        }

        public double getActualCurrent()
        {
            int mVolts = current.Read();

            return mVolts * ampScale;//mAmps
        }

        public double getActualVoltage()
        {
            int mVolts = voltage.Read();

            return mVolts * voltScale / 1000;
        }

        public ushort getEstimation()
        {
            int perCent = getBatteryCapacity();
            double current = getActualCurrent();

            if (current > 0)
                return (ushort)(mAh * perCent * 60 / (current * 100));//minutes
            else
                return 0;
        }
    }
}
