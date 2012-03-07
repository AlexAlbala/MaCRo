using System;
using Microsoft.SPOT;
using MaCRo.Tools;
using MaCRo.Config;

namespace MaCRo.Drivers.IMU
{
    public class DCM
    {
        public nIMU adc;


        //float GRAVITY = 101;//this equivalent to 1G in the raw data coming from the accelerometer 
        //#define Accel_Scale(x) x*(GRAVITY/9.81)//Scaling the raw data of the accel to actual acceleration in meters for seconds square

        int OUTPUTMODE = 1;

        /**************SLOW****************/
        //float Kp_ROLLPITCH = 0.015f;
        //float Ki_ROLLPITCH = 0.000010f;
        //float Kp_YAW = 1.2f;
        //float Ki_YAW = 0.00005f;
        /**************SLOW****************/

        /******************************/
        float Kp_ROLLPITCH = 1f;
        float Ki_ROLLPITCH = 0.001f;
        float Kp_YAW = 5f;      //High yaw drift correction gain - use with caution!
        float Ki_YAW = 0.05f;
        /*****************************/
        float[][] DCM_Matrix;

        float G_Dt = 0.02f;    // Integration time (DCM algorithm)

        long timeNow = 0; // Hold the milliseond value for now
        long timer = 0;   //general purpuse timer
        long timer_old;
        long timer24 = 0; //Second timer used to print values 
        bool groundstartDone = false;    // Used to not repeat ground start

        float[] AN = new float[8]; //array that store the 6 ADC filtered data
        float[] AN_OFFSET = new float[8]; //Array that stores the Offset of the gyros

        public float[] Accel_Vector = { 0, 0, 0 }; //Store the acceleration in a vector
        public float[] Gyro_Vector = { 0, 0, 0 };//Store the gyros rutn rate in a vector

        float[] Omega_Vector = { 0, 0, 0 }; //Corrected Gyro_Vector data
        float[] Omega_P = { 0, 0, 0 };//Omega Proportional correction
        float[] Omega_I = { 0, 0, 0 };//Omega Integrator
        float[] Omega = { 0, 0, 0 };

        // Euler angles
        public float roll{get; set;}
        public float pitch { get; set; }
        public float yaw { get; set; }

        public float MAG_Heading { get; set; }

        int toggleMode = 0;

        float[] errorRollPitch = { 0, 0, 0 };
        float[] errorYaw = { 0, 0, 0 };
        float errorCourse = 180;
        float COGX = 0; //Course overground X axis
        float COGY = 1; //Course overground Y axis

        uint cycleCount = 0;
        byte gyro_sat = 0;

        float[][] Update_Matrix;// = { new float[] { 0, 1, 2 }, new float[] { 3, 4, 5 }, new float[] { 6, 7, 8 } }; //Gyros here

        float[][] Temporary_Matrix;// = { new float[] { 0, 0, 0 }, new float[] { 0, 0, 0 }, new float[] { 0, 0, 0 } };

        // Startup GPS variables
        int gps_fix_count = 5;		//used to count 5 good fixes at ground startup



        public DCM(nIMU imu)
        {
            this.adc = imu;

            DCM_Matrix = new float[3][];
            DCM_Matrix[0] = new float[] { 1, 0, 0 };
            DCM_Matrix[1] = new float[] { 0, 1, 0 };
            DCM_Matrix[2] = new float[] { 0, 0, 1 };

            Temporary_Matrix = new float[3][];
            Temporary_Matrix[0] = new float[] { 0, 0, 0 };
            Temporary_Matrix[1] = new float[] { 0, 0, 0 };
            Temporary_Matrix[2] = new float[] { 0, 0, 0 };

            Update_Matrix = new float[3][];
            Update_Matrix[0] = new float[] { 0, 1, 2 };
            Update_Matrix[1] = new float[] { 3, 4, 5 };
            Update_Matrix[2] = new float[] { 6, 7, 8 };
        }
        /**************************************************/
        public void Normalize()
        {
            float error = 0;

            float[][] temporary = new float[3][];
            temporary[0] = new float[3];// { 0, 0, 0 };
            temporary[1] = new float[3]; //{ 0, 0, 0 };
            temporary[2] = new float[3]; //{ 0, 0, 0 };

            //for (int i = 0; i < 3; i++)
            //{
            //    for (int j = 0; j < 3; j++)
            //    {
            //        temporary[i][j] = new float();
            //        temporary[i][j] = 0.1f;
            //    }
            //}
            ////temporary[0][1] = 3.2f;

            float renorm = 0;
            bool problem = false;

            error = -1 * Vector.Vector_Dot_Product(DCM_Matrix[0], DCM_Matrix[1]) * .5f; //eq.19

            Vector.Vector_Scale(temporary[0], DCM_Matrix[1], error); //eq.19
            Vector.Vector_Scale(temporary[1], DCM_Matrix[0], error); //eq.19

            Vector.Vector_Add(temporary[0], temporary[0], DCM_Matrix[0]);//eq.19
            Vector.Vector_Add(temporary[1], temporary[1], DCM_Matrix[1]);//eq.19

            Vector.Vector_Cross_Product(temporary[2], temporary[0], temporary[1]); // c= a x b //eq.20

            renorm = Vector.Vector_Dot_Product(temporary[0], temporary[0]);
            if (renorm < 1.5625f && renorm > 0.64f)
            {
                renorm = .5f * (3 - renorm);                                                 //eq.21
            }
            else if (renorm < 100.0f && renorm > 0.01f)
            {
                renorm = 1.0f / (float)exMath.Sqrt(renorm);
                //#if PERFORMANCE_REPORTING == 1  
                //    renorm_sqrt_count++;
                //#endif
                //#if PRINT_DEBUG != 0
                //    Serial.print("???SQT:1,RNM:");
                //    Serial.print (renorm);
                //    Serial.print (",ERR:");
                //    Serial.print (error);
                //    Serial.print (",TOW:");
                //    Serial.print (GPS.time);  
                //    Serial.println("***");    
                //#endif
            }
            else
            {
                problem = true;
                //#if PERFORMANCE_REPORTING == 1
                //    renorm_blowup_count++;
                //#endif
                //    #if PRINT_DEBUG != 0
                //    Serial.print("???PRB:1,RNM:");
                //    Serial.print (renorm);
                //    Serial.print (",ERR:");
                //    Serial.print (error);
                //    Serial.print (",TOW:");
                //    Serial.print (GPS.time);  
                //    Serial.println("***");    
                //#endif
            }
            Vector.Vector_Scale(DCM_Matrix[0], temporary[0], renorm);

            renorm = Vector.Vector_Dot_Product(temporary[1], temporary[1]);
            if (renorm < 1.5625f && renorm > 0.64f)
            {
                renorm = .5f * (3 - renorm);                                                 //eq.21
            }
            else if (renorm < 100.0f && renorm > 0.01f)
            {
                renorm = 1.0f / (float)exMath.Sqrt(renorm);
                //#if PERFORMANCE_REPORTING == 1    
                //    renorm_sqrt_count++;
                //#endif
                //#if PRINT_DEBUG != 0
                //    Serial.print("???SQT:2,RNM:");
                //    Serial.print (renorm);
                //    Serial.print (",ERR:");
                //    Serial.print (error);
                //    Serial.print (",TOW:");
                //    Serial.print (GPS.time);  
                //    Serial.println("***");    
                //#endif
            }
            else
            {
                problem = true;
                //#if PERFORMANCE_REPORTING == 1
                //    renorm_blowup_count++;
                //#endif
                //#if PRINT_DEBUG != 0
                //    Serial.print("???PRB:2,RNM:");
                //    Serial.print (renorm);
                //    Serial.print (",ERR:");
                //    Serial.print (error);
                //    Serial.print (",TOW:");
                //    Serial.print (GPS.time);  
                //    Serial.println("***");    
                //#endif
            }
            Vector.Vector_Scale(DCM_Matrix[1], temporary[1], renorm);

            renorm = Vector.Vector_Dot_Product(temporary[2], temporary[2]);
            if (renorm < 1.5625f && renorm > 0.64f)
            {
                renorm = .5f * (3 - renorm);                                                 //eq.21
            }
            else if (renorm < 100.0f && renorm > 0.01f)
            {
                renorm = 1.0f / (float)exMath.Sqrt(renorm);
                //#if PERFORMANCE_REPORTING == 1 
                //    renorm_sqrt_count++;
                //#endif
                //#if PRINT_DEBUG != 0
                //    Serial.print("???SQT:3,RNM:");
                //    Serial.print (renorm);
                //    Serial.print (",ERR:");
                //    Serial.print (error);
                //    Serial.print (",TOW:");
                //    Serial.print (GPS.time);  
                //    Serial.println("***");    
                //#endif
            }
            else
            {
                problem = true;
                //#if PERFORMANCE_REPORTING == 1
                //    renorm_blowup_count++;
                //#endif
                //#if PRINT_DEBUG != 0
                //    Serial.print("???PRB:3,RNM:");
                //    Serial.print (renorm);
                //    Serial.print (",TOW:");
                //    Serial.print (GPS.time);  
                //    Serial.println("***");    
                //#endif
            }
            Vector.Vector_Scale(DCM_Matrix[2], temporary[2], renorm);

            if (problem)
            {                // Our solution is blowing up and we will force back to initial condition.  Hope we are not upside down!
                DCM_Matrix[0][0] = 1.0f;
                DCM_Matrix[0][1] = 0.0f;
                DCM_Matrix[0][2] = 0.0f;
                DCM_Matrix[1][0] = 0.0f;
                DCM_Matrix[1][1] = 1.0f;
                DCM_Matrix[1][2] = 0.0f;
                DCM_Matrix[2][0] = 0.0f;
                DCM_Matrix[2][1] = 0.0f;
                DCM_Matrix[2][2] = 1.0f;
                problem = false;
            }
        }

        /**************************************************/
        public void Drift_correction()
        {
            //Compensation the Roll, Pitch and Yaw drift. 
            float mag_heading_x;
            float mag_heading_y;
            float[] Scaled_Omega_P = new float[3];
            float[] Scaled_Omega_I = new float[3];
            float Accel_magnitude;
            float Accel_weight;
            float Integrator_magnitude;
            float tempfloat;

            //*****Roll and Pitch***************

            // Calculate the magnitude of the accelerometer vector
            Accel_magnitude = (float)exMath.Sqrt(Accel_Vector[0] * Accel_Vector[0] + Accel_Vector[1] * Accel_Vector[1] + Accel_Vector[2] * Accel_Vector[2]);
            Accel_magnitude = Accel_magnitude / GlobalVal.gravity; // Scale to gravity.
            // Dynamic weighting of accelerometer info (reliability filter)
            // Weight for accelerometer info (<0.5G = 0.0, 1G = 1.0 , >1.5G = 0.0)
            Accel_weight = 1 - 2 * (float)exMath.Abs(1 - Accel_magnitude);  // 
            if (Accel_weight > 1)
                Accel_weight = 1;
            if (Accel_weight < 0)
                Accel_weight = 0;

            //#if PERFORMANCE_REPORTING == 1
            //  tempfloat = ((Accel_weight - 0.5) * 256.0f);    //amount added was determined to give imu_health a time constant about twice the time constant of the roll/pitch drift correction
            //  imu_health += tempfloat;
            //  imu_health = constrain(imu_health,129,65405);
            //#endif

            Vector.Vector_Cross_Product(errorRollPitch, Accel_Vector, DCM_Matrix[2]); //adjust the ground of reference
            Vector.Vector_Scale(Omega_P, errorRollPitch, Kp_ROLLPITCH * Accel_weight);

            Vector.Vector_Scale(Scaled_Omega_I, errorRollPitch, Ki_ROLLPITCH * Accel_weight);
            Vector.Vector_Add(Omega_I, Omega_I, Scaled_Omega_I);


            //*****YAW***************


            //#if USE_MAGNETOMETER==1 


            double[] mag = adc.getMag();

            //http://code.google.com/p/sf9domahrs/
            float CMx = (float)(mag[0] * exMath.Cos(this.pitch) + mag[1] * exMath.Sin(this.roll) * exMath.Sin(this.pitch) + mag[2] * exMath.Cos(this.roll) * exMath.Sin(this.pitch));
            float CMy = (float)(mag[1] * exMath.Cos(this.roll) - mag[2] * exMath.Sin(this.roll));

            MAG_Heading = (float)exMath.Atan(CMy / CMx);
            //  // We make the gyro YAW drift correction based on compass magnetic heading
            //errorCourse=(DCM_Matrix[0][0]*AP_Compass.Heading_Y) - (DCM_Matrix[1][0]*AP_Compass.Heading_X);  //Calculating YAW error
            //Vector_Scale(errorYaw,&DCM_Matrix[2][0],errorCourse); //Applys the yaw correction to the XYZ rotation of the aircraft, depeding the position.

            //Vector_Scale(&Scaled_Omega_P[0],&errorYaw[0],Kp_YAW);
            //Vector_Add(Omega_P,Omega_P,Scaled_Omega_P);//Adding  Proportional.

            //Vector_Scale(&Scaled_Omega_I[0],&errorYaw[0],Ki_YAW);
            //Vector_Add(Omega_I,Omega_I,Scaled_Omega_I);//adding integrator to the Omega_I   
            //#else  // Use GPS Ground course to correct yaw gyro drift
            //if(GPS.ground_speed>=SPEEDFILT*100)		// Ground speed from GPS is in m/s
            //{
            //COGX = exMath.Cos(ToRad(GPS.ground_course/100.0));
            //COGY = exMath.Sin(ToRad(GPS.ground_course/100.0));            

            COGX = (float)exMath.Cos((float)MAG_Heading);
            COGY = (float)exMath.Sin((float)MAG_Heading);


            errorCourse = (DCM_Matrix[0][0] * COGY) - (DCM_Matrix[1][0] * COGX);  //Calculating YAW error
            Vector.Vector_Scale(errorYaw, DCM_Matrix[2], errorCourse); //Applys the yaw correction to the XYZ rotation of the aircraft, depeding the position.

            Vector.Vector_Scale(Scaled_Omega_P, errorYaw, Kp_YAW);
            Vector.Vector_Add(Omega_P, Omega_P, Scaled_Omega_P);//Adding  Proportional.

            Vector.Vector_Scale(Scaled_Omega_I, errorYaw, Ki_YAW);
            Vector.Vector_Add(Omega_I, Omega_I, Scaled_Omega_I);//adding integrator to the Omega_I   
            //}
            //#endif
            ////  Here we will place a limit on the integrator so that the integrator cannot ever exceed half the saturation limit of the gyros
            
            Integrator_magnitude = (float)exMath.Sqrt(Vector.Vector_Dot_Product(Omega_I, Omega_I));
            if (Integrator_magnitude > exMath.ToRad(300))
            {
                Vector.Vector_Scale(Omega_I, Omega_I, 0.5f * exMath.ToRad(300) / Integrator_magnitude);
                //#if PRINT_DEBUG != 0
                //    Serial.print("!!!INT:1,MAG:");
                //    Serial.print (ToDeg(Integrator_magnitude));

                //    Serial.print (",TOW:");
                //    Serial.print (GPS.time);  
                //    Serial.println("***");    
                //#endif
            }


        }
        /**************************************************/
        void Accel_adjust()
        {
            //Accel_Vector[1] += Accel_Scale((GPS.ground_speed/100)*Omega[2]);  // Centrifugal force on Acc_y = GPS ground speed (m/s) * GyroZ
            //Accel_Vector[2] -= Accel_Scale((GPS.ground_speed/100)*Omega[1]);  // Centrifugal force on Acc_z = GPS ground speed (m/s) * GyroY 

        }
        /**************************************************/

        public void Matrix_update()
        {
            double[] gyro = adc.getGyro();
            //Gyro_Vector[0]=Gyro_Scaled_X(read_adc(0)); //gyro x roll
            //Gyro_Vector[1]=Gyro_Scaled_Y(read_adc(1)); //gyro y pitch
            //Gyro_Vector[2]=Gyro_Scaled_Z(read_adc(2)); //gyro Z yaw

            Gyro_Vector[0] = (float)gyro[0];
            Gyro_Vector[1] = (float)gyro[1];
            Gyro_Vector[2] = (float)gyro[2];


            double[] acc = adc.getAccel();
            //Accel_Vector[0]=read_adc(3); // acc x
            //Accel_Vector[1]=read_adc(4); // acc y
            //Accel_Vector[2]=read_adc(5); // acc z
            Accel_Vector[0] = (float)acc[0];
            Accel_Vector[1] = (float)acc[1];
            Accel_Vector[2] = (float)acc[2];

            Vector.Vector_Add(Omega, Gyro_Vector, Omega_I);  //adding proportional term
            Vector.Vector_Add(Omega_Vector, Omega, Omega_P); //adding Integrator term

            Accel_adjust();    //Remove centrifugal acceleration.

            if (OUTPUTMODE == 1)
            {
                Update_Matrix[0][0] = 0;
                Update_Matrix[0][1] = -G_Dt * Omega_Vector[2];//-z
                Update_Matrix[0][2] = G_Dt * Omega_Vector[1];//y
                Update_Matrix[1][0] = G_Dt * Omega_Vector[2];//z
                Update_Matrix[1][1] = 0;
                Update_Matrix[1][2] = -G_Dt * Omega_Vector[0];//-x
                Update_Matrix[2][0] = -G_Dt * Omega_Vector[1];//-y
                Update_Matrix[2][1] = G_Dt * Omega_Vector[0];//x
                Update_Matrix[2][2] = 0;
            }
            else                    // Uncorrected data (no drift correction)
            {
                Update_Matrix[0][0] = 0;
                Update_Matrix[0][1] = -G_Dt * Gyro_Vector[2];//-z
                Update_Matrix[0][2] = G_Dt * Gyro_Vector[1];//y
                Update_Matrix[1][0] = G_Dt * Gyro_Vector[2];//z
                Update_Matrix[1][1] = 0;
                Update_Matrix[1][2] = -G_Dt * Gyro_Vector[0];
                Update_Matrix[2][0] = -G_Dt * Gyro_Vector[1];
                Update_Matrix[2][1] = G_Dt * Gyro_Vector[0];
                Update_Matrix[2][2] = 0;
            }

            Matrix.Matrix_Multiply(DCM_Matrix, Update_Matrix, Temporary_Matrix); //a*b=c

            for (int x = 0; x < 3; x++) //Matrix Addition (update)
            {
                for (int y = 0; y < 3; y++)
                {
                    DCM_Matrix[x][y] += Temporary_Matrix[x][y];
                }
            }
        }

        public void Euler_angles()
        {
            if (OUTPUTMODE == 2)
            {       // Only accelerometer info (debugging purposes)
                roll = (float)exMath.Atan2(Accel_Vector[1], Accel_Vector[2]);    // atan2(acc_y,acc_z)
                pitch = -1 * (float)exMath.Asin((Accel_Vector[0]) / GlobalVal.gravity); // asin(acc_x)
                yaw = 0;
            }
            else
            {
                pitch = -1 * (float)exMath.Asin(DCM_Matrix[2][0]);
                roll = (float)exMath.Atan2(DCM_Matrix[2][1], DCM_Matrix[2][2]);
                yaw = (float)exMath.Atan2(DCM_Matrix[1][0], DCM_Matrix[0][0]);
            }

        }


    }
}
