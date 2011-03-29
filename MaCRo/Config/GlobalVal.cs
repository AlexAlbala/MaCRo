using System;
using Microsoft.SPOT;

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
        public readonly static sbyte speed = 22;
        /// <summary>
        /// Velocity of the wheel during a turn
        /// </summary>
        public readonly static sbyte turningSpeed = 35;
        /// <summary>
        /// Cell width and height
        /// </summary>
        public readonly static ushort cellSize_cm = 15;
        /// <summary>
        /// The initial width and height of the scenario
        /// </summary>
        public readonly static ushort initialMapSize_cm = 1000;
    }
}
