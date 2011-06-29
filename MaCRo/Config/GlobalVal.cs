using System;
using Microsoft.SPOT;
using MaCRo.Tools;

namespace MaCRo.Config
{
    public static class GlobalVal
    {
        /// <summary>
        /// Perimeter of the wheel in millimeters
        /// </summary>
        public readonly static short wheelPerimeter_mm = 200;
        /// <summary>
        /// Number of interruptions in every turn of the wheel
        /// </summary>
        public readonly static short interruptsWheel = 20;
        /// <summary>
        /// Width of the structure. Center of the wheel to center of the wheel
        /// </summary>
        public readonly static short width_mm = 148;
        /// <summary>
        /// Length of the structure.
        /// </summary>
        public readonly static short length_mm = 235;
        /// <summary>
        /// The radius of the turn
        /// </summary>
        public readonly static short turnRadius = width_mm;
        /// <summary>
        /// Distance between the front bumper and the center of the front wheels (in millimeters)
        /// </summary>
        public readonly static short bumperToWheel_mm = 70;
        /// <summary>
        /// Natural speed of the rover
        /// </summary>
        public readonly static sbyte speed = 25;
        /// <summary>
        /// Velocity of the wheel during a turn
        /// </summary>
        public readonly static sbyte turningSpeed = 30;
        /// <summary>
        /// Distance for an object to be detected. In centimeters
        /// </summary>
        public readonly static short distanceToDetect = 25;       
        /// <summary>
        /// Distance which has to be respected when following the wall. In centimeters
        /// </summary>
        public readonly static short minDistanceToFollowWall = 7;//(int)exMath.Ceiling(length_mm / 10);
        /// <summary>
        /// Centimeters of hysteresis when following the wall.
        /// </summary>
        public readonly static short hysteresis = 2;
        /// <summary>
        /// Value of acceleration in m/s of 1g
        /// </summary>
        public readonly static float gravity = 9.807f;
        /// <summary>
        /// The time between the contingency takes a measure sample
        /// </summary>
        public readonly static short contingencyPeriod = 1000;
        /// <summary>
        /// Time while the robot is correcting any trouble
        /// </summary>
        public readonly static short contingencyCorrectionTime = 500;

        /// <summary>
        /// The periodicity of the sensor telemetry transmission in milliseconds
        /// </summary>
        public readonly static short transmissionPeriodSensor_ms = 300;
        /// <summary>
        /// The periodicity of the Position telemetry transmission in milliseconds
        /// </summary>
        public readonly static short transmissionPeriodPosition_ms = 250;
        /// <summary>
        /// The periodicity of the IMU telemetry transmission in milliseconds
        /// </summary>
        public readonly static short transmissionPeriodIMU_ms = 100;
        /// <summary>
        /// The periodicity of the Temperature telemetry transmission in milliseconds
        /// </summary>
        public readonly static short transmissionPeriodTemp_ms = 30000;


        /// <summary>
        /// Periodicity of the IMU data update 
        /// </summary>
        public readonly static short imuUpdate_ms = 40;
        /// <summary>
        /// Time between samples of the integration of the IMU data
        /// </summary>
        public readonly static short integrationPeriod = 25;
        /// <summary>
        /// Minimum value of acceleration (m/s^2) to take into account
        /// </summary>
        public readonly static double accelerationThreshold = 0.1;
        /// <summary>
        /// Coefficient of the low pass filter
        /// </summary>
        public readonly static float IIRFilterCoefficient = 0.75f;

        /// <summary>
        /// Periodicity of the main loop of the DCM algorithm
        /// </summary>
        public readonly static int DCMLoopPeriodicity = 20;
    }
}
