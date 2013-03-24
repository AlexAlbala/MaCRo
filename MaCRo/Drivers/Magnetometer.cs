using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;
using MaCRo.Tools;
using MaCRo.Core;

namespace MaCRo.Drivers
{
    public enum Magnetome
    {
        magX,
        magY

    }
    class Magnetometer
    {
        private I2CDevice magnetometer;
        private short freq = 80;
        private ushort address = 0x0E;
        private Timer update;
        private short bufferSize = 7;
        private NavigationManager navigation;
        public double MAGHeadingRad { get; set; }
        public double MAGHeadingDeg { get; set; }

        short magX = 0, magY = 0, magZ = 0;

        private object monitor;

        public Magnetometer()
        {
            monitor = new object();
            magnetometer = new I2CDevice(new I2CDevice.Configuration(address, freq));
            //I2CDevice.I2CWriteTransaction activeAutoReset_RAWMode = I2CDevice.CreateWriteTransaction(new byte[] { 0x12, 0xC0 });
            I2CDevice.I2CWriteTransaction activeMode = I2CDevice.CreateWriteTransaction(new byte[] { 0x10, 0x01 });
            I2CDevice.I2CWriteTransaction correct = I2CDevice.CreateWriteTransaction(new byte[] { 0x11, 0x00 });
            //I2CDevice.I2CWriteTransaction activeMode_FastMode = I2CDevice.CreateWriteTransaction(new byte[] { 0x10, 0x05 });
            int i = magnetometer.Execute(new I2CDevice.I2CTransaction[] { activeMode, correct }, 1000);

            //2285
            //-3062
            byte[] XOFF = FromShortC2(2630);
            I2CDevice.I2CWriteTransaction XOFFM = I2CDevice.CreateWriteTransaction(new byte[] { 0x09, XOFF[1] });
            I2CDevice.I2CWriteTransaction XOFFL = I2CDevice.CreateWriteTransaction(new byte[] { 0x0A, XOFF[0] });

            byte[] YOFF = FromShortC2(-3192);
            I2CDevice.I2CWriteTransaction YOFFM = I2CDevice.CreateWriteTransaction(new byte[] { 0x0B, YOFF[1] });
            I2CDevice.I2CWriteTransaction YOFFL = I2CDevice.CreateWriteTransaction(new byte[] { 0x0C, YOFF[0] });

            int i2 = magnetometer.Execute(new I2CDevice.I2CTransaction[] { XOFFM, XOFFL, YOFFM, YOFFL }, 1000);

            update = new Timer(new TimerCallback(read), new object(), 1000, 500);
        }

        public void read(Object state)
        {
            monitor = new object();
            byte[] buffer = new byte[bufferSize];
            I2CDevice.I2CWriteTransaction transactionWrite = I2CDevice.CreateWriteTransaction(new byte[] { 0x01 });
            I2CDevice.I2CReadTransaction transactionRead = I2CDevice.CreateReadTransaction(buffer);
            int i = magnetometer.Execute(new I2CDevice.I2CTransaction[] { transactionWrite, transactionRead }, 1000);

            if (i == 8 && buffer[6] == 0xC4)
            {
                //X
                magX = ToShortC2(new byte[] { buffer[1], buffer[0] });
                //Y
                magY = ToShortC2(new byte[] { buffer[3], buffer[2] });
                //Z
                magZ = ToShortC2(new byte[] { buffer[5], buffer[4] });

               MAGHeadingRad = exMath.Atan2(-1 * magY, magX);
                /*
                if ((magX == 0) && (magY < 0))
                    MAGHeadingRad = exMath.PI / 2.0;
                if ((magX == 0) && (magY > 0))
                    MAGHeadingRad = 3.0 * exMath.PI / 2.0;
                if (magX < 0)
                    MAGHeadingRad = exMath.PI - exMath.Atan2(magX, magY);
                if ((magX > 0) && (magY < 0))
                    MAGHeadingRad = -exMath.Atan2(magX, magY);
                if ((magX > 0) && (magY > 0))
                    MAGHeadingRad = 2.0 * exMath.PI - exMath.Atan2(magX, magY);
                */
                MAGHeadingDeg = (float)exMath.ToDeg((float)MAGHeadingRad);

                Debug.Print("X: " + magX.ToString() + " Y: " + magY.ToString() + " Z: " + magZ.ToString() + " " + MAGHeadingDeg.ToString());
                //Debug.Print(buffer[0] + " " + buffer[1] + " " + buffer[2] + " " + buffer[3]);
                //Debug.Print("Y: " + magY.ToString());
                //Engine.getInstance().Debug("X: " + magX.ToString() + " Y: " + magY.ToString() + " " + MAGHeadingDeg.ToString());
            }

        }

        public short[] getMag()
        {
            lock (monitor)
            {
                return new short[] { magX, magY, magZ };
            }
        }

        public void SetXOFF(short xoff)
        {
            Engine.getInstance().Info("Fin x" + xoff);
            byte[] XOFF = FromShortC2(xoff);
            I2CDevice.I2CWriteTransaction XOFFM = I2CDevice.CreateWriteTransaction(new byte[] { 0x09, XOFF[1] });
            I2CDevice.I2CWriteTransaction XOFFL = I2CDevice.CreateWriteTransaction(new byte[] { 0x0A, XOFF[0] });

            int i = magnetometer.Execute(new I2CDevice.I2CTransaction[] { XOFFM, XOFFL }, 1000);
            Debug.Print("X: " + XOFF.ToString());
        }

        public void SetYOFF(short yoff)
        {
            Engine.getInstance().Info("Fin y" + yoff);
            byte[] YOFF = FromShortC2(yoff);
            I2CDevice.I2CWriteTransaction YOFFM = I2CDevice.CreateWriteTransaction(new byte[] { 0x0B, YOFF[1] });
            I2CDevice.I2CWriteTransaction YOFFL = I2CDevice.CreateWriteTransaction(new byte[] { 0x0C, YOFF[0] });

            Engine.getInstance().Info("Y1:" + YOFF[1] + "Y0:" + YOFF[0]);

            int i = magnetometer.Execute(new I2CDevice.I2CTransaction[] { YOFFM, YOFFL }, 1000);
            Debug.Print("Y:" + YOFF.ToString());
        }

        private short ToShortC2(byte[] buffer)
        {
            return (short)(
               (buffer[0] & 0x000000FF) |
               (buffer[1] << 8 & 0x0000FF00)
               );
        }

        private byte[] FromShortC2(short theShort)
        {
            byte[] buffer = new byte[2];
            unchecked
            {
                buffer[0] = (byte)(theShort << 1);
                buffer[1] = (byte)(theShort >> 7);
            }
            return buffer;
        }
    }
}
