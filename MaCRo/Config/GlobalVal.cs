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
        public readonly static int wheelPerimeter_mm = 200;
        /// <summary>
        /// Number of interruptions in every turn of the wheel
        /// </summary>
        public readonly static int interruptsWheel = 20;
        /// <summary>
        /// Width of the structure. Center of the wheel to center of the wheel
        /// </summary>
        public readonly static int width_mm = 148;
        /// <summary>
        /// Length of the structure.
        /// </summary>
        public readonly static int length_mm = 235;
        /// <summary>
        /// The radius of the turn
        /// </summary>
        public readonly static int turnRadius = width_mm;
        /// <summary>
        /// Distance between the front bumper and the center of the front wheels (in millimeters)
        /// </summary>
        public readonly static int bumperToWheel_mm = 70;
        /// <summary>
        /// Natural speed of the rover
        /// </summary>
        public readonly static sbyte speed = 25;
        /// <summary>
        /// Velocity of the wheel during a tur0
        /// </summary>
        public readonly static sbyte turningSpeed = 30;
        /// <summary>
        /// Distance for an object to be detected. In centimeters
        /// </summary>
        public readonly static int distanceToDetect = 24;       
        /// <summary>
        /// Distance which has to be respected when following the wall. In centimeters
        /// </summary>
        public readonly static int distanceToFollowWall = 7;//(int)exMath.Ceiling(length_mm / 10);
        /// <summary>
        /// Centimeters of hysteresis when following the wall. In centimeters
        /// </summary>
        public readonly static int hysteresis = 1;
        /// <summary>
        /// Inclination of the right sensor
        /// </summary>
        public readonly static int rightAngle = 45;
        /// <summary>
        /// Inclination of the left sensor
        /// </summary>
        public readonly static int leftAngle = 45;
        /// <summary>
        /// The periodicity of the telemetry transmission in milliseconds
        /// </summary>
        public readonly static int transmissionPeriod_ms = 100;
        /// <summary>
        /// Periodicity of the IMU data update 
        /// </summary>
        public readonly static int imuUpdate_ms = 50;





        ///// <summary>
        ///// Cell width and height
        ///// </summary>
        //public readonly static ushort cellSize_cm = 15;
        ///// <summary>
        ///// The initial width and height of the scenario
        ///// </summary>
        //public readonly static ushort initialMapSize_cm = 1000;
    }
}
