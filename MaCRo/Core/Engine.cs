using System;
using Microsoft.SPOT;
using System.Threading;
using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT.Hardware;
using MaCRo.Config;
using MaCRo.Tools;
using MaCRo.Communications;

namespace MaCRo.Core
{
    class Engine
    {
        private static Engine instance;

        private OutputPort debug;

        //private MapManager map;
        private NavigationManager navigation;
        private SensorManager sensors;
        private Coder coder;

        private Mode currentMode;

        private Position actualPosition;

        private Timer timer;

        public static Engine getInstance()
        {
            if (instance == null)
                instance = new Engine();
            return instance;
        }

        public void InitializeSystem()
        {
            navigation = new NavigationManager();
            sensors = new SensorManager();
            coder = new Coder();
            coder.Start();

            currentMode = Mode.SearchingForWall;
            debug = new OutputPort((Cpu.Pin)PortMap.debug, false);

            timer = new Timer(new TimerCallback(timer_tick), new object(), 0, GlobalVal.transmissionPeriod_ms);
        }

        void timer_tick(Object state)
        {
            debug.Write(true);
            ushort l1 = (ushort)(sensors.getDistance(Sensor.Central) * 10);
            ushort s2 = (ushort)(sensors.getDistance(Sensor.Wall) * 10);
            ushort s1 = (ushort)(sensors.getDistance(Sensor.wall_back) * 10);
            ushort l2 = (ushort)(sensors.getDistance(Sensor.Right) * 10);

            coder.Send(Message.SensorS1, s1);
            coder.Send(Message.SensorS2, s2);
            coder.Send(Message.SensorL1, l1);
            coder.Send(Message.SensorL2, l2);
            
            debug.Write(false);
        }

        public Engine()
        {
            //map = new MapManager();

            actualPosition = new Position();
            actualPosition.x = 0;
            actualPosition.y = 0;
        }

        public void Run()
        {
            //int i = 0;
            //while (true)
            //{
            //    coder.Send(Message.MoveForward, (ushort)i++);
            //    Thread.Sleep(1000);

            //    //debug LED !
            //    debug.Write(true);
            //    Thread.Sleep(100);
            //    debug.Write(false);
            //}

            while (true)
            {
                //debug LED !
                debug.Write(true);
                Thread.Sleep(100);
                debug.Write(false);

                if (currentMode == Mode.SearchingForWall)
                {
                    coder.Send(Message.Mode, 0);
                    navigation.MoveToObject(sensors);

                    float central = sensors.getDistance(Sensor.Central);
                    float left = sensors.getDistance(Sensor.wall_back);
                    float right = sensors.getDistance(Sensor.Right);

                    coder.Send(Message.MoveForward, (ushort)navigation.distance_mm);

                    if (central <= GlobalVal.distanceToDetect)
                    {
                        //WALL FOUNDED
                        navigation.resetDistance();
                        navigation.MoveBackward(GlobalVal.width_mm - (int)central * 10, (sbyte)GlobalVal.speed);
                        coder.Send(Message.MoveBackward, (ushort)navigation.distance_mm);
                        navigation.turnRight(90);
                        currentMode = Mode.FollowWall;

                        coder.Send(Message.TurnRight, (ushort)90);
                    }
                    continue;
                }
                else if (currentMode == Mode.FollowWall)
                {
                    coder.Send(Message.Mode, 1);
                    float central = sensors.getDistance(Sensor.Central);
                    float right = sensors.getDistance(Sensor.Right);
                    float wall = sensors.getDistance(Sensor.Wall);
                    float wallback = sensors.getDistance(Sensor.wall_back);

                    //FOLLOW LEFT WALL
                    while (true)
                    {
                        wall = sensors.getDistance(Sensor.Wall);
                        wallback = sensors.getDistance(Sensor.wall_back);

                        if (exMath.Abs(wall - wallback) < GlobalVal.hysteresis)
                        {
                            if (wall > (GlobalVal.distanceToFollowWall + GlobalVal.hysteresis))
                            {
                                coder.Send(Message.MoveForward, (ushort)navigation.distance_mm);
                                navigation.turnLeft();
                                Thread.Sleep(100);
                                continue;
                            }
                            else if (wall < (GlobalVal.distanceToFollowWall - GlobalVal.hysteresis))
                            {
                                coder.Send(Message.MoveForward, (ushort)navigation.distance_mm);
                                navigation.turnRight();
                                Thread.Sleep(100);
                                continue;
                            }
                        }
                        else if (wall < wallback)
                        {
                            navigation.turnRight();
                            Thread.Sleep(100);
                        }
                        else if (wall > wallback)
                        {
                            navigation.turnLeft();
                            Thread.Sleep(100);
                        }

                        //if (wall > (GlobalVal.distanceToFollowWall + GlobalVal.hysteresis))
                        //{
                        //    coder.Send(Message.MoveForward, (ushort)navigation.distance_mm);
                        //    navigation.turnLeft();
                        //    Thread.Sleep(200);
                        //    navigation.turnRight();
                        //    Thread.Sleep(200);
                        //    continue;
                        //}
                        //else if (wall < (GlobalVal.distanceToFollowWall - GlobalVal.hysteresis))
                        //{
                        //    coder.Send(Message.MoveForward, (ushort)navigation.distance_mm);
                        //    navigation.turnRight();
                        //    Thread.Sleep(100);
                        //    continue;
                        //}
                        //else
                        //{
                        //    navigation.MoveForward();
                        //}                        
                    }

                    right = sensors.getDistance(Sensor.Right);
                    //MaCRo is approaching a new wall
                    if (right > GlobalVal.distanceToContinue)
                    {
                        navigation.MoveBackward(GlobalVal.wheelPerimeter_mm - (int)central * 10, (sbyte)GlobalVal.speed);
                        navigation.turnRight(90);
                        continue;
                    }
                    else
                    {
                        //MACRO BLOCKED !!!
                        //CHANGE MODE
                    }
                }


                /*
                if (central < 10)
                {
                    navigation.MoveBackward();
                    //dm.brake();
                }
                else if (central > 12)
                {
                    if (left < 13 || right < 13)
                    {
                        if (left < 13)
                        {
                            navigation.turnRight();
                            Thread.Sleep(100);
                        }

                        if (right < 13)
                        {
                            navigation.turnLeft();
                            Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        navigation.MoveForward();
                    }

                }
                else
                {
                    navigation.brake();
                }*/

            }

        }

    }
}
