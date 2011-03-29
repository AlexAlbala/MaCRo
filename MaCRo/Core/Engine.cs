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
            map = new MapManager();
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

                    float central = 0;
                    float left = 0;
                    float right = 0;
                    central = sensors.getDistance(Sensor.Central);
                    left = sensors.getDistance(Sensor.Left);
                    right = sensors.getDistance(Sensor.Right);
                   


                }

                float central = 0;
                float left = 0;
                float right = 0;

                for (int i = 0; i < 5; i++)
                {
                    central += sensors.getDistance(Sensor.Central);
                    left += sensors.getDistance(Sensor.Left);
                    right += sensors.getDistance(Sensor.Right);
                }

                central /= 5;
                left /= 5;
                right /= 5;

                if (central <= 30 && central >= 4 && central >= GlobalVal.cellSize_cm)
                {
                    if (left > central && right > central)
                    {
                        navigation.MoveToObject(sensors);
                        double distanceMoved = navigation.distance_mm;
                        int cellsMoved = (int)(distanceMoved / (GlobalVal.cellSize_cm * 10));

                        switch (navigation.getActualOrientation())
                        {
                            case Orientation.HorizontalPos:
                                actualPosition.x += cellsMoved;
                                break;
                            case Orientation.HorizontalNeg:
                                actualPosition.x -= cellsMoved;
                                break;
                            case Orientation.VerticalPos:
                                actualPosition.y += cellsMoved;
                                break;
                            case Orientation.VerticalNeg:
                                actualPosition.y -= cellsMoved;
                                break;
                        }

                        //map.SetMapped(actualPosition);

                        continue;
                    }
                    else
                    {
                        //THERE ARE SURROUNDING OBJECTS
                    }
                }
                else
                {
                    if (central <= GlobalVal.cellSize_cm)
                    {
                        //There is an object here. Mark it !
                        //map.SetObject(actualPosition);
                        //map.SetMapped(actualPosition);
                        navigation.MoveBackward(GlobalVal.width_mm - (int)central * 10, (sbyte)GlobalVal.speed);

                        if (right > GlobalVal.cellSize_cm)//AND THE RIGHT CELL IS NOT MAPPED
                        {
                            navigation.turnRight(90);
                            navigation.orientationToRight();
                            continue;
                        }
                        else if (left > GlobalVal.cellSize_cm)//AND THE LEFT CELL IS NOT MAPPED
                        {
                            navigation.turnLeft(90);
                            navigation.orientationToLeft();
                            continue;
                        }
                        else
                        {
                            //ROBOT IS BLOCKED. DO SOMETHING
                            continue;
                        }
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
