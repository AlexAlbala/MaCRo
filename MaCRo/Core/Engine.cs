using System;
using Microsoft.SPOT;
using System.Threading;
using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT.Hardware;
using MaCRo.Config;
using MaCRo.Tools;

namespace MaCRo.Core
{
    class Engine
    {
        private static Engine instance;

        private OutputPort debug;

        //private MapManager map;
        private NavigationManager navigation;
        private SensorManager sensors;

        private Mode currentMode;

        private Position actualPosition;

        public static Engine getInstance()
        {
            if (instance == null)
                instance = new Engine();
            return instance;
        }

        public Engine()
        {
            currentMode = Mode.SearchingForWall;
            //map = new MapManager();
            navigation = new NavigationManager();
            sensors = new SensorManager();

            debug = new OutputPort((Cpu.Pin)PortMap.debug, false);

            actualPosition = new Position();
            actualPosition.x = 0;
            actualPosition.y = 0;
        }

        public void Run()
        {
            while (true)
            {
                //debug LED !
                debug.Write(true);
                Thread.Sleep(100);
                debug.Write(false);

                if (currentMode == Mode.SearchingForWall)
                {
                    navigation.MoveToObject(sensors);

                    float central = sensors.getDistance(Sensor.Central);
                    float left = sensors.getDistance(Sensor.Left);
                    float right = sensors.getDistance(Sensor.Right);

                    if (central <= GlobalVal.distanceToDetect)
                    {
                        //WALL FOUNDED
                        navigation.MoveBackward(GlobalVal.width_mm - (int)central * 10, (sbyte)GlobalVal.speed);
                        navigation.turnRight(90);
                        currentMode = Mode.FollowWall;
                    }
                    continue;
                }
                else if (currentMode == Mode.FollowWall)
                {
                    float central = sensors.getDistance(Sensor.Central);
                    float left = sensors.getDistance(Sensor.Left);
                    float right = sensors.getDistance(Sensor.Right);
                    //FOLLOW LEFT WALL
                    while (true)
                    {
                        //debug LED !
                        debug.Write(true);
                        Thread.Sleep(100);
                        debug.Write(false);

                        navigation.MoveForward();
                        left = sensors.getDistance(Sensor.Left);

                        if (left > (GlobalVal.distanceToFollowWall + GlobalVal.hysteresis))
                        {
                            navigation.turnLeft();
                            Thread.Sleep(100);
                            continue;
                        }
                        else if (left < GlobalVal.distanceToFollowWall - GlobalVal.hysteresis)
                        {
                            navigation.turnRight();
                            Thread.Sleep(100);
                            continue;

                        }
                        else
                        {
                            continue;
                        }
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
