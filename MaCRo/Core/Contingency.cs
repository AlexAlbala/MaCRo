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
        private Random r;
        public bool alarm;
        Thread th;
        Thread th2;

        public Contingency(Encoder left, Encoder right, NavigationManager navigation)
        {
            alarm = false;
            this.left = left;
            this.right = right;
            this.navigation = navigation;
            r = new Random();

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
                    Engine.getInstance().Debug("Solving problem when moving " + movement.ToString());
                    switch (movement)
                    {
                        case Movement.forward:
                            if (r.NextDouble() > 0.8)
                                navigation.MoveBackward(100);
                            else
                            {
                                if (r.NextDouble() > 0.5)
                                    navigation.turnRight(10);
                                else
                                    navigation.turnLeft(10);
                            }
                            break;
                        case Movement.backward:
                            if (r.NextDouble() > 0.8)
                                navigation.MoveForward(100);
                            else
                            {
                                if (r.NextDouble() > 0.5)
                                    navigation.turnRight(10);
                                else
                                    navigation.turnLeft(10);
                            }
                            break;
                        case Movement.left:
                            if (r.NextDouble() > 0.8)
                                navigation.MoveForward(100);
                            else
                                navigation.turnRight(10, (sbyte)50);
                            break;
                        case Movement.right:
                            if (r.NextDouble() > 0.8)
                                navigation.MoveForward(100);
                            else
                                navigation.turnLeft(10, (sbyte)50);
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
                        Engine.getInstance().Debug("Contingency alarm");
                        movement = navigation.movement;
                        alarm = true;
                        Engine.getInstance().Cancel();
                    }
                }
            }
        }
    }
}
