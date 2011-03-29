using System;
using Microsoft.SPOT;

namespace MaCRo.Core
{
    class State
    {
        /// <summary>
        /// Position in the x axis
        /// </summary>
        private int _xpos;
        /// <summary>
        /// Position in the y axis
        /// </summary>
        private int _ypos;
        /// <summary>
        /// Looking angle
        /// </summary>
        private double _angle;
        /// <summary>
        /// Velocity
        /// </summary>
        private int _velocity;

        public int xpos { get { return _xpos; } set { _xpos = value; } }

        public int ypos { get { return _ypos; } set { _ypos = value; } }

        public double angle { get { return _angle; } set { _angle = value; } }

        public int velocity { get { return _velocity; } set { _velocity = value; } }
    }
    class KalmanFilter
    {
        /// <summary>
        /// Measurement noise
        /// </summary>
        private int R;
        /// <summary>
        /// Process noise
        /// </summary>
        private int Q;
        /// <summary>
        /// State vector
        /// </summary>
        private State[] states;

        private int numMaxStates;

        private int currentState;

        public KalmanFilter()
        {
            numMaxStates = 150;
            currentState = 0;
            states = new State[5];

            states[0] = new State();
            states[0].angle = 0;
            states[0].velocity = 0;
            states[0].xpos = 0;
            states[0].ypos = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="angle"></param>
        public void Run(int distance, double angle)
        {
            State previous = states[currentState++];

            //Compute next state
            State actual = new State();
            actual.xpos = previous.xpos + (int)MaCRo.Tools.exMath.Cos(angle) * distance;
            actual.ypos = previous.ypos + (int)MaCRo.Tools.exMath.Sin(angle) * distance;
            actual.angle = angle;




        }

    }
}
