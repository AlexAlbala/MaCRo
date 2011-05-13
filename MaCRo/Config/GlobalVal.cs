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
        public readonly static sbyte speed = 24;
        /// <summary>
        /// Velocity of the wheel during a turn
        /// </summary>
        public readonly static sbyte turningSpeed = 30;
        /// <summary>
        /// Distance for an object to be detected. In centimeters
        /// </summary>
        public readonly static short distanceToDetect = 23;       
        /// <summary>
        /// Distance which has to be respected when following the wall. In centimeters
        /// </summary>
        public readonly static short distanceToFollowWall = 7;//(int)exMath.Ceiling(length_mm / 10);
        /// <summary>
        /// Centimeters of hysteresis when following the wall. In centimeters
        /// </summary>
        public readonly static short hysteresis = 2;
        /// <summary>
        /// Inclination of the right sensor
        /// </summary>
        public readonly static int rightAngle = 45;
        /// <summary>
        /// Inclination of the left sensor
        /// </summary>
        public readonly static short leftAngle = 45;
        /// <summary>
        /// The periodicity of the telemetry transmission in milliseconds
        /// </summary>
        public readonly static short transmissionPeriod_ms = 100;
        /// <summary>
        /// The periodicity of the telemetry transmission in milliseconds
        /// </summary>
        public readonly static short transmissionPeriodPosition_ms = 500;
        /// <summary>
        /// Periodicity of the IMU data update 
        /// </summary>
        public readonly static short imuUpdate_ms = 25;
        /// <summary>
        /// Time between samples of the integration of the IMU data
        /// </summary>
        public readonly static short integrationPeriod = 30;
    }
}
