using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;
using MaCRo.Config;


namespace MaCRo.Drivers
{
    class nIMU
    {
        private string name = "NA05 0300F050C";

        private I2CDevice nimu;
        private short bufferSize = 38;
        private ushort address = 113;
        private ushort freq = 100;
        private Timer t;

        private double digitalSensitivityGyro = 1.3733e-2;
        private double digitalSensitivtyAccel = 2.2888e-4;
        private double digitalSensitivityMag = 8.6975e-5;
        private double digitalSensitivityTemp = 1.8165e-2;

        //{0} = X || {1} = Y || {2} = Z
        private short[] lastAcc;
        private short[] lastGyr;
        private short[] lastMag;
        private short[] lastTemp;

        public nIMU()
        {
            lastAcc = new short[3];
            lastGyr = new short[3];
            lastTemp = new short[3];
            lastMag = new short[3];

            nimu = new I2CDevice(new I2CDevice.Configuration(address, freq));
            
        }

        public void Start()
        {
            t = new Timer(new TimerCallback(t_Tick), new object(), 0, GlobalVal.imuUpdate_ms);
        }

        public void Stop()
        {
            t.Dispose();
            t = null;
        }

        void t_Tick(Object state)
        {
            byte[] buffer = new byte[bufferSize];
            I2CDevice.I2CReadTransaction transactionRead = I2CDevice.CreateReadTransaction(buffer);
            int i = nimu.Execute(new I2CDevice.I2CTransaction[] { transactionRead }, 1000);

            if (i == bufferSize)
            {
                if (buffer[0] == (byte)255 && buffer[1] == (byte)255 && buffer[2] == (byte)255 && buffer[3] == (byte)255)
                {
                    //X
                    lastAcc[0] = ToShortC2(new byte[] { buffer[20], buffer[19] }, 0);
                    //Y
                    lastAcc[1] = ToShortC2(new byte[] { buffer[22], buffer[21] }, 0);
                    //Z
                    lastAcc[2] = ToShortC2(new byte[] { buffer[24], buffer[23] }, 0);

                    //X
                    lastGyr[0] = ToShortC2(new byte[] { buffer[14], buffer[13] }, 0);
                    //Y
                    lastGyr[1] = ToShortC2(new byte[] { buffer[16], buffer[15] }, 0);
                    //Z
                    lastGyr[2] = ToShortC2(new byte[] { buffer[18], buffer[17] }, 0);

                    //X
                    lastMag[0] = ToShortC2(new byte[] { buffer[26], buffer[25] }, 0);
                    //Y
                    lastMag[1] = ToShortC2(new byte[] { buffer[28], buffer[27] }, 0);
                    //Z
                    lastMag[2] = ToShortC2(new byte[] { buffer[30], buffer[29] }, 0);

                    //X
                    lastTemp[0] = ToShortC2(new byte[] { buffer[32], buffer[31] }, 0);
                    //Y
                    lastTemp[1] = ToShortC2(new byte[] { buffer[34], buffer[33] }, 0);
                    //Z
                    lastTemp[2] = ToShortC2(new byte[] { buffer[36], buffer[35] }, 0);
                }
                else
                {
 
                }
            }

        }

        public short[] getAccel()
        {
            return lastAcc;           
        }

        public short[] getGyro()
        {
            return lastGyr;
        }

        public short[] getTemp()
        {
            return lastTemp;
        }

        public short[] getMag()
        {
            return lastMag;
        }
       
        private short ToShortC2(byte[] buffer, int offset)
        {
            return (short)(
               (buffer[offset] & 0x000000FF) |
               (buffer[offset + 1] << 8 & 0x0000FF00)
               );
        }
    }
}
