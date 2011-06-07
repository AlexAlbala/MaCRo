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
        private InputPort encoder;

        private double _distance;
        private double stepmm;
        private bool lastValue;

        public double distance_mm
        {
            get { return _distance; }
        }

        public Encoder(FEZ_Pin.Digital pin)
        {
            //encoder = new InterruptPort((Cpu.Pin)pin, true, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeHigh);
            //encoder.OnInterrupt += new NativeEventHandler(encoder_OnInterrupt);
            encoder = new InputPort((Cpu.Pin)pin, true, Port.ResistorMode.PullDown);
            stepmm = GlobalVal.wheelPerimeter_mm / GlobalVal.interruptsWheel;
            this.resetDistance();

            Thread th = new Thread(new ThreadStart(this.Run));
            th.Start();

        }

        private void Run()
        {
            lastValue = encoder.Read();
            while (true)
            {
                bool actualValue = encoder.Read();
                if (actualValue == lastValue)
                {
                    continue;
                }
                else
                {
                    _distance += stepmm;
                    lastValue = actualValue;
                    Debug.Print("PIN: " + this.encoder.Id.ToString() + " - Distance: " + _distance.ToString());
                }
            }
        }
        public void resetDistance()
        {            
            _distance = 0;
        }

        //void encoder_OnInterrupt(uint pin, uint value, DateTime time)
        //{
        //    //Debug.Print("Weba: " + pin + " " + time.ToString());
        //    _distance += stepmm;

        //    Debug.Print("PIN: " + pin.ToString() + " - " + _distance.ToString());
        //    //TimeSpan ts = time - lastLecture;
        //    //if (ts.Milliseconds > glitchFilterWidth_ms)
        //    //{
        //    //    lastLecture = time;
        //    //    _distance += stepmm;
        //    //}
        //}

    }
}
