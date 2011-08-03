using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;
using MaCRo.Config;
using MaCRo.Tools;


namespace MaCRo.Drivers.IMU
{
    public class nIMU
    {
        private string name = "NA05 0300F050C";

        private I2CDevice nimu;
        private short bufferSize = 38;
        private ushort address = 113;
        private ushort freq = 100;
        private Timer t;
        private Timer dcm_timer;


        private DCM dcm;
        private float G_Dt;
        private int cycleCount;
        private int gyro_sat;

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
        private double lastValueTimer;
        private long counter;
        private bool iirInitialized;

        private double oax;
        private double oay;
        private double oaz;

        public float Yaw { get { return dcm.yaw; } }
        public float Pitch { get { return dcm.pitch; } }
        public float Roll { get { return dcm.roll; } }
        public float MAG_Heading { get { return dcm.MAG_Heading; } }

        private DateTime lastDCMLoopTime;

        private object monitor;

        public nIMU()
        {
            //dcm = new DCM(this);
            lastAcc = new double[3];
            lastGyr = new double[3];
            lastTemp = new double[3];
            lastMag = new double[3];
            lastTime = 0.0;
            lastValueTimer = 0.0;
            counter = 0;
            iirInitialized = false;
            oax = oay = oaz = 0.0;
            lastDCMLoopTime = DateTime.MinValue;

            rangeTimer = ushort.MaxValue * digitalSensitivityTime;
            monitor = new object();
            nimu = new I2CDevice(new I2CDevice.Configuration(address, freq));
            G_Dt = 0.02f;
            cycleCount = 0;
            gyro_sat = 0;

        }

        public void Start()
        {
            t = new Timer(new TimerCallback(t_Tick), new object(), 0, GlobalVal.imuUpdate_ms);
            //dcm_timer = new Timer(new TimerCallback(loop), new object(), 0, GlobalVal.DCMLoopPeriodicity);
        }

        public void Stop()
        {
            t.Dispose();
            t = null;
        }

        double[] IIR3AxisFilter(double ax, double ay, double az)
        {
            double bx = ax;
            double by = ay;
            double bz = az;

            if (iirInitialized)
            {
                ax = oax + (bx - oax) * GlobalVal.IIRFilterCoefficient;
                ay = oay + (by - oay) * GlobalVal.IIRFilterCoefficient;
                az = oaz + (bz - oaz) * GlobalVal.IIRFilterCoefficient;
            }

            oax = ax;
            oay = ay;
            oaz = az;
            iirInitialized = true;

            return new double[] { ax, ay, az };
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

                        lastValueTimer = tempTime;
                        lastTime = tempTime + (rangeTimer * counter);

                        //X
                        lastAcc[0] = GlobalVal.gravity * digitalSensitivtyAccel * ToShortC2(new byte[] { buffer[20], buffer[19] });
                        //Y
                        lastAcc[1] = GlobalVal.gravity * digitalSensitivtyAccel * ToShortC2(new byte[] { buffer[22], buffer[21] });
                        //Z
                        lastAcc[2] = GlobalVal.gravity * digitalSensitivtyAccel * ToShortC2(new byte[] { buffer[24], buffer[23] });

                        //lastAcc = accel_filter(lastAcc[0], lastAcc[1], lastAcc[2]);
                        //Debug.Print(lastAcc[0].ToString() + " - " + lastAcc[1].ToString() + " - " + lastAcc[2].ToString());

                        //X
                        lastGyr[0] = (double)exMath.ToRad((float)(digitalSensitivityGyro * ToShortC2(new byte[] { buffer[14], buffer[13] })));
                        //Y
                        lastGyr[1] = (double)exMath.ToRad((float)(digitalSensitivityGyro * ToShortC2(new byte[] { buffer[16], buffer[15] })));
                        //Z
                        lastGyr[2] = (double)exMath.ToRad((float)(digitalSensitivityGyro * ToShortC2(new byte[] { buffer[18], buffer[17] })));

                        //X
                        lastMag[0] = digitalSensitivityMag * ToShortC2(new byte[] { buffer[26], buffer[25] });
                        //Y
                        lastMag[1] = digitalSensitivityMag * ToShortC2(new byte[] { buffer[28], buffer[27] });
                        //Z
                        lastMag[2] = digitalSensitivityMag * ToShortC2(new byte[] { buffer[30], buffer[29] });

                        lastMag = IIR3AxisFilter(lastMag[0], lastMag[1], lastMag[2]);

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

        //public void setCorrectAccel(double x, double y, double z)
        //{
        //    lastAcc[0] = x;
        //    lastAcc[1] = y;
        //    lastAcc[2] = z;
        //}

        //public void setCorrectGyro(double x, double y, double z)
        //{
        //    lastGyr[0] = x;
        //    lastGyr[1] = y;
        //    lastGyr[2] = z;
        //}

        public double[] getAccel()
        {
            lock (monitor)
            {
                return lastAcc;
                //return new double[] { dcm.Accel_Vector[0], dcm.Accel_Vector[1], dcm.Accel_Vector[2] };
            }
        }

        public double[] getGyro()
        {
            lock (monitor)
            {
                return lastGyr;
                //return new double[] { dcm.Gyro_Vector[0], dcm.Gyro_Vector[1], dcm.Gyro_Vector[2] };
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

        private void Read_adc_raw()
        {
            this.t_Tick(new object());
        }

        private void loop(Object state) //Main Loop
        {

            if (lastDCMLoopTime == DateTime.MinValue)
            {
                lastDCMLoopTime = DateTime.Now;
            }
            else
            {
                DateTime now = DateTime.Now;
                TimeSpan diff = now - lastDCMLoopTime;

                double millis = 1000.0 * diff.Ticks / Cpu.SystemClock;
                if (millis >= 20)
                {
                    lastDCMLoopTime = now;
                    G_Dt = (float)millis / 1000.0f;
                }
                else
                {
                    return;
                }
            }

            //if((timeNow-timer)>=20)  // Main loop runs at 50Hz
            //{
            //  timer_old = timer;
            //  timer = timeNow;

            //  G_Dt = (timer-timer_old)/1000.0;    // Real time of loop run. We use this on the DCM algorithm (gyro integration time)
            //  if(G_Dt > 1)
            //      G_Dt = 0;  //Something is wrong - keeps dt from blowing up, goes to zero to keep gyros from departing

            // *** DCM algorithm

            Read_adc_raw();

            dcm.Matrix_update();

            dcm.Normalize();

            dcm.Drift_correction();

            dcm.Euler_angles();

            lastAcc[0] = dcm.Accel_Vector[0];
            lastAcc[1] = dcm.Accel_Vector[1];
            lastAcc[2] = dcm.Accel_Vector[2];

            lastGyr[0] = dcm.Gyro_Vector[0];
            lastGyr[1] = dcm.Gyro_Vector[1];
            lastGyr[2] = dcm.Gyro_Vector[2];

            //Turn on the LED when you saturate any of the gyros.
            if ((exMath.Abs(dcm.Gyro_Vector[0]) >= exMath.ToRad(300)) || (exMath.Abs(dcm.Gyro_Vector[1]) >= exMath.ToRad(300)) || (exMath.Abs(dcm.Gyro_Vector[2]) >= exMath.ToRad(300)))
            {
                gyro_sat = 1;
                //digitalWrite(5,HIGH); //???  
            }

            cycleCount++;

            // Do these things every 6th time through the main cycle 
            // This section gets called every 1000/(20*6) = 8 1/3 Hz
            // doing it this way removes the need for another 'millis()' call
            // and balances the processing load across main loop cycles.
            //switch (cycleCount) {
            //    case(0):
            //        //GPS.update();
            //        break;

            //    case(1):
            //        //Here we will check if we are getting a signal to ground start
            //        if(digitalRead(GROUNDSTART_PIN) == LOW && groundstartDone == false) 
            //            startup_ground();
            //        break;

            //    case(2):
            //        //#if USE_BAROMETER==1
            //        //    ReadSCP1000();    // Read I2C absolute pressure sensor
            //        //    press_filt = (press + 2l * press_filt) / 3l;		//Light filtering
            //        //    //temperature = (temperature * 9 + temp_unfilt) / 10;    We will just use the ground temp for the altitude calculation
            //        //#endif
            //        break;

            //    case(3):
            //        //#if USE_MAGNETOMETER==1
            //        //APM_Compass.Read();     // Read magnetometer
            //        //APM_Compass.Calculate(roll,pitch);  // Calculate heading 
            //        //#endif
            //        break;

            //    case(4):
            //        // Display Status on LEDs
            //        // GYRO Saturation indication
            //        if(gyro_sat>=1) {
            //            digitalWrite(5,HIGH); //Turn Red LED when gyro is saturated. 
            //            if(gyro_sat>=8)  // keep the LED on for 8/10ths of a second
            //                gyro_sat=0;
            //            else
            //                gyro_sat++;
            //        } else {
            //            digitalWrite(5,LOW);
            //        }

            //        // YAW drift correction indication
            //        if(GPS.ground_speed<SPEEDFILT*100) {
            //            digitalWrite(7,HIGH);    //  Turn on yellow LED if speed too slow and yaw correction supressed
            //        } else {
            //            digitalWrite(7,LOW);
            //        }

            //        // GPS Fix indication
            //                        switch (GPS.status()) {
            //                                case(2):
            //                  digitalWrite(6,HIGH);  //Turn Blue LED when gps is fixed. 
            //                                      break;

            //                                case(1):
            //                                      if (GPS.valid_read == true){
            //                                            toggleMode = abs(toggleMode-1); // Toggle blue light on and off to indicate NMEA sentences exist, but no GPS fix lock
            //                                            if (toggleMode==0){
            //                                                  digitalWrite(6, LOW); // Blue light off
            //                                            } else {
            //                                                  digitalWrite(6, HIGH); // Blue light on
            //                                            }
            //                                            GPS.valid_read = false;
            //                                      }
            //                                      break;

            //                                default:
            //                                      digitalWrite(6,LOW);
            //                                      break;
            //        }
            //        break;

            //    case(5):
            //        cycleCount = -1;		// Reset case counter, will be incremented to zero before switch statement
            //        #if !PRINT_BINARY
            //            printdata(); //Send info via serial
            //        #endif
            //        break;
            //}

        }

    }
}

