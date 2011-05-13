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

        //{0} = X || {1} = Y || {2} = Z
        private double[] lastAcc;
        private double[] lastGyr;
        private double[] lastMag;
        private double[] lastTemp;
        private double lastTime;

        private double digitalSensitivityGyro = 1.3733e-2;
        private double digitalSensitivtyAccel = 2.2888e-4;
        private double digitalSensitivityMag = 8.6975e-5;
        private double digitalSensitivityTemp = 1.8165e-2;
        private double digitalSensitivityTime = 2.1701e-6;

        private double rangeTimer;
        private long counter;

        private object monitor;

        public nIMU()
        {
            lastAcc = new double[3];
            lastGyr = new double[3];
            lastTemp = new double[3];
            lastMag = new double[3];
            lastTime = 0.0;
            counter = 0;

            rangeTimer = ushort.MaxValue * digitalSensitivityTime;
            monitor = new object();
            nimu = new I2CDevice(new I2CDevice.Configuration(address, freq));

        }

        public void Start()
        {
            t = new Timer(new TimerCallback(t_Filter_Tick), new object(), 0, GlobalVal.imuUpdate_ms);
        }

        public void Stop()
        {
            t.Dispose();
            t = null;
        }

        void t_Filter_Tick(Object state)
        {
            lock (monitor)
            {
                double acx = 0.0;
                double acy = 0.0;
                double acz = 0.0;

                double gx = 0.0;
                double gy = 0.0;
                double gz = 0.0;

                double tx = 0.0;
                double ty = 0.0;
                double tz = 0.0;

                double mx = 0.0;
                double my = 0.0;
                double mz = 0.0;

                int m = 0;
                while (m < 5)
                {
                    byte[] buffer = new byte[bufferSize];
                    I2CDevice.I2CReadTransaction transactionRead = I2CDevice.CreateReadTransaction(buffer);
                    int i = nimu.Execute(new I2CDevice.I2CTransaction[] { transactionRead }, 1000);

                    if (i == bufferSize)
                    {
                        if (buffer[0] == (byte)255 && buffer[1] == (byte)255 && buffer[2] == (byte)255 && buffer[3] == (byte)255)
                        {
                            double tempTime = digitalSensitivityTime * ToUShortC2(new byte[] { buffer[8], buffer[7] });

                            if (tempTime < lastTime)
                            {
                                counter++;
                            }
                            lastTime = tempTime + (rangeTimer * counter);

                            //X
                            acx += digitalSensitivtyAccel * ToShortC2(new byte[] { buffer[20], buffer[19] });
                            //Y
                            acy += digitalSensitivtyAccel * ToShortC2(new byte[] { buffer[22], buffer[21] });
                            //Z
                            acz += digitalSensitivtyAccel * ToShortC2(new byte[] { buffer[24], buffer[23] });

                            //X
                            gx += digitalSensitivityGyro * ToShortC2(new byte[] { buffer[14], buffer[13] });
                            //Y
                            gy += digitalSensitivityGyro * ToShortC2(new byte[] { buffer[16], buffer[15] });
                            //Z
                            gz += digitalSensitivityGyro * ToShortC2(new byte[] { buffer[18], buffer[17] });

                            //X
                            mx += digitalSensitivityMag * ToShortC2(new byte[] { buffer[26], buffer[25] });
                            //Y
                            my += digitalSensitivityMag * ToShortC2(new byte[] { buffer[28], buffer[27] });
                            //Z
                            mz += digitalSensitivityMag * ToShortC2(new byte[] { buffer[30], buffer[29] });

                            //X
                            tx += digitalSensitivityTemp * ToShortC2(new byte[] { buffer[32], buffer[31] }) + 25;
                            //Y
                            ty += digitalSensitivityTemp * ToShortC2(new byte[] { buffer[34], buffer[33] }) + 25;
                            //Z
                            tz += digitalSensitivityTemp * ToShortC2(new byte[] { buffer[36], buffer[35] }) + 25;

                            m++;
                        }
                        else
                        {
                            Debug.Print("Invalid IMU data");
                        }
                    }
                }

                lastAcc[0] = acx / m;
                lastAcc[1] = acy / m;
                lastAcc[2] = acz / m;

                lastGyr[0] = gx / m;
                lastGyr[1] = gy / m;
                lastGyr[2] = gz / m;

                lastTemp[0] = tx / m;
                lastTemp[1] = ty / m;
                lastTemp[2] = tz / m;

                lastMag[0] = mx / m;
                lastMag[1] = my / m;
                lastMag[2] = mz / m;
            }
        }

        void t_Tick(Object state)
        {
            lock (monitor)
            {
                byte[] buffer = new byte[bufferSize];
                I2CDevice.I2CReadTransaction transactionRead = I2CDevice.CreateReadTransaction(buffer);
                int i = nimu.Execute(new I2CDevice.I2CTransaction[] { transactionRead }, 1000);

                if (i == bufferSize)
                {
                    if (buffer[0] == (byte)255 && buffer[1] == (byte)255 && buffer[2] == (byte)255 && buffer[3] == (byte)255)
                    {
                        double tempTime = digitalSensitivityTime * ToUShortC2(new byte[] { buffer[8], buffer[7] });

                        if (tempTime < lastTime)
                        {
                            counter++;
                        }
                        lastTime = tempTime + (rangeTimer * counter);

                        //X
                        lastAcc[0] = digitalSensitivtyAccel * ToShortC2(new byte[] { buffer[20], buffer[19] });
                        //Y
                        lastAcc[1] = digitalSensitivtyAccel * ToShortC2(new byte[] { buffer[22], buffer[21] });
                        //Z
                        lastAcc[2] = digitalSensitivtyAccel * ToShortC2(new byte[] { buffer[24], buffer[23] });

                        //X
                        lastGyr[0] = digitalSensitivityGyro * ToShortC2(new byte[] { buffer[14], buffer[13] });
                        //Y
                        lastGyr[1] = digitalSensitivityGyro * ToShortC2(new byte[] { buffer[16], buffer[15] });
                        //Z
                        lastGyr[2] = digitalSensitivityGyro * ToShortC2(new byte[] { buffer[18], buffer[17] });

                        //X
                        lastMag[0] = digitalSensitivityMag * ToShortC2(new byte[] { buffer[26], buffer[25] });
                        //Y
                        lastMag[1] = digitalSensitivityMag * ToShortC2(new byte[] { buffer[28], buffer[27] });
                        //Z
                        lastMag[2] = digitalSensitivityMag * ToShortC2(new byte[] { buffer[30], buffer[29] });

                        //X
                        lastTemp[0] = digitalSensitivityTemp * ToShortC2(new byte[] { buffer[32], buffer[31] }) + 25;
                        //Y
                        lastTemp[1] = digitalSensitivityTemp * ToShortC2(new byte[] { buffer[34], buffer[33] }) + 25;
                        //Z
                        lastTemp[2] = digitalSensitivityTemp * ToShortC2(new byte[] { buffer[36], buffer[35] }) + 25;
                    }
                    else
                    {

                    }
                }
            }
        }

        public double getTime()
        {
            lock (monitor)
            {
                return lastTime;
            }
        }

        public double[] getAccel()
        {
            lock (monitor)
            {
                return lastAcc;
            }
        }

        public double[] getGyro()
        {
            lock (monitor)
            {
                return lastGyr;
            }
        }

        public double[] getTemp()
        {
            lock (monitor)
            {
                return lastTemp;
            }
        }

        public double[] getMag()
        {
            lock (monitor)
            {
                return lastMag;
            }
        }

        private short ToShortC2(byte[] buffer)
        {
            return (short)(
               (buffer[0] & 0x000000FF) |
               (buffer[1] << 8 & 0x0000FF00)
               );
        }

        private ushort ToUShortC2(byte[] buffer)
        {
            return (ushort)(
               (buffer[0] & 0x000000FF) |
               (buffer[1] << 8 & 0x0000FF00)
               );
        }
    }
}
