using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using System.Threading;
using MaCRo.Config;

namespace MaCRo.Drivers
{
    public class Encoder
    {
        private InterruptPort encoder;

        private double _distance;
        private double stepmm;
        private DateTime lastLecture;
        private int glitchFilterWidth_ms;

        public double distance_mm
        {
            get { return _distance; }
        }

        public Encoder(FEZ_Pin.Interrupt pin)
        {
            encoder = new InterruptPort((Cpu.Pin)pin, true, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeBoth);
            encoder.OnInterrupt += new NativeEventHandler(encoder_OnInterrupt);

            stepmm = GlobalVal.wheelPerimeter_mm / GlobalVal.interruptsWheel;
            lastLecture = new DateTime();
            this.resetDistance();
            glitchFilterWidth_ms = 135;       
        }

        public void resetDistance()
        {
            _distance = 0;
        }

        void encoder_OnInterrupt(uint pin, uint value, DateTime time)
        {
            TimeSpan ts = time - lastLecture;
            if (ts.Milliseconds > glitchFilterWidth_ms)
            {
                lastLecture = time;
                _distance += stepmm;
            }
        }

    }
}
