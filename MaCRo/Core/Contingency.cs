using System;
using Microsoft.SPOT;
using MaCRo.Drivers;
using System.Threading;
using MaCRo.Tools;
using MaCRo.Config;

namespace MaCRo.Core
{
    public class Contingency
    {
        private Encoder left;
        private Encoder right;
        private NavigationManager navigation;
        private Movement movement;
        public bool alarm;
        Thread th;
        Thread th2;

        public Contingency(Encoder left, Encoder right, NavigationManager navigation)
        {
            alarm = false;
            this.left = left;
            this.right = right;
            this.navigation = navigation;

            th = new Thread(new ThreadStart(Run));
            th.Start();

            th2 = new Thread(new ThreadStart(Act));
            th2.Start();
        }

        public void Disable()
        {
            th.Abort();
            th2.Abort();
        }

        public void Restart()
        {
            th = new Thread(new ThreadStart(Run));
            th.Start();

            th2 = new Thread(new ThreadStart(Act));
            th2.Start();
        }

        public void Act()
        {
            while (true)
            {
                if (alarm)
                {
                    switch (movement)
                    {
                        case Movement.forward:
                            navigation.MoveBackward(100);
                            break;
                        case Movement.backward:
                        case Movement.left:
                        case Movement.right:
                            navigation.MoveForward(100);
                            break;
                    }

                    Engine.getInstance().Restart();
                    alarm = false;
                }
            }
        }

        public void Run()
        {
            while (true)
            {
                if (navigation.movement != Movement.stop && !alarm)
                {
                    double rightDistance = right.distance_mm;
                    double leftDistance = left.distance_mm;

                    Thread.Sleep(GlobalVal.contingencyPeriod);

                    if (right.distance_mm == rightDistance && left.distance_mm == leftDistance)
                    {
                        movement = navigation.movement;
                        alarm = true;
                        Engine.getInstance().Cancel();              
                    }
                }
            }
        }
    }
}
