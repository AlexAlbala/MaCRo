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
        private int count;
        private Timer t;
        private object monitor;
        private bool end;

        public Contingency(Encoder left, Encoder right, NavigationManager navigation)
        {
            alarm = false;
            monitor = new object();
            this.left = left;
            this.right = right;
            this.navigation = navigation;
            count = 0;
            end = false;

            th = new Thread(new ThreadStart(Run));
            th.Start();

            th2 = new Thread(new ThreadStart(Act));
            th2.Start();
            t = new Timer(new TimerCallback(decount), new object(), 0, 5000);
        }

        public void decount(Object state)
        {
            lock (monitor)
            {
                if (count > 0)
                    count--;
            }
        }

        public void Disable()
        {
            end = true;
            th.Abort();
            th2.Abort();
        }

        public void Restart()
        {
            end = false;
            th = new Thread(new ThreadStart(Run));
            th.Start();
            

            th2 = new Thread(new ThreadStart(Act));
            th2.Start();
        }

        public void Act()
        {
            while (!end)
            {
                if (alarm)
                {
                    lock (monitor)
                    {
                        if (count >= 5)
                        {
                            Engine.getInstance().Debug("Contingency count: " + count);
                            if (count >= 10)
                            {                                
                                navigation.turnLeft(20, (sbyte)90);
                            }
                            else if (count >= 15)
                            {
                                navigation.turnRight(20, (sbyte)90);
                                count = 10;
                            }
                            else
                                navigation.MoveForward(100, (sbyte)90);
                        }
                    }
                    Engine.getInstance().Debug("Solving problem when moving " + movement.ToString());
                    switch (movement)
                    {
                        case Movement.forward:
                            navigation.MoveBackward(100, (sbyte)40);
                            break;
                        case Movement.backward:
                            navigation.MoveForward(100, (sbyte)40);
                            break;
                        case Movement.left:
                            navigation.turnRight(10, (sbyte)40);
                            break;
                        case Movement.right:
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
            while (!end)
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
                        lock (monitor)
                        {
                            count++;
                        }
                        Engine.getInstance().Cancel();
                    }
                }
            }
        }
    }
}
