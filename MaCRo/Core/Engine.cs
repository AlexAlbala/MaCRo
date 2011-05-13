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
        private Timer positionTimer;

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
            positionTimer = new Timer(new TimerCallback(posTimer_Tick), new object(), 0, GlobalVal.transmissionPeriodPosition_ms);
        }

        void posTimer_Tick(Object state)
        {
            lock (coder)
            {
                Position pos = navigation.getActualPosition();
                Position vel = navigation.getActualVelocity();

                coder.Send(Message.PositionX, pos.x);
                coder.Send(Message.PositionY, pos.y);
                coder.Send(Message.Angle, pos.angle);

                coder.Send(Message.VelocityX, vel.x);
                coder.Send(Message.VelocityY, vel.y);

                coder.Send(Message.Time, navigation.getTime());

            }
        }

        void timer_tick(Object state)
        {
            debug.Write(true);

            short l1 = (short)(sensors.getDistance(Sensor.Central) * 10);
            short s2 = (short)(sensors.getDistance(Sensor.Wall) * 10);
            short s1 = (short)(sensors.getDistance(Sensor.wall_back) * 10);
            short l2 = (short)(sensors.getDistance(Sensor.Right) * 10);

            lock (coder)
            {
                if (s1 > 40 && s1 < 300)
                    coder.Send(Message.SensorS1, s1);
                if (s2 > 40 && s2 < 300)
                    coder.Send(Message.SensorS2, s2);
                if (l1 > 100 && l1 < 800)
                    coder.Send(Message.SensorL1, l1);
                if (l2 > 100 && l2 < 800)
                    coder.Send(Message.SensorL2, l2);

                //IMU Telemetry
                coder.Send(Message.IMUMagX, navigation.getMag(Axis.X));
                coder.Send(Message.IMUMagY, navigation.getMag(Axis.Y));
                coder.Send(Message.IMUMagZ, navigation.getMag(Axis.Z));

                coder.Send(Message.IMUGyrX, navigation.getGyro(Axis.X));
                coder.Send(Message.IMUGyrY, navigation.getGyro(Axis.Y));
                coder.Send(Message.IMUGyrZ, navigation.getGyro(Axis.Z));

                coder.Send(Message.IMUTempX, navigation.getTemp(Axis.X));
                coder.Send(Message.IMUTempY, navigation.getTemp(Axis.Y));
                coder.Send(Message.IMUTempZ, navigation.getTemp(Axis.Z));

                coder.Send(Message.IMUAccX, navigation.getAccel(Axis.X));
                coder.Send(Message.IMUAccY, navigation.getAccel(Axis.Y));
                coder.Send(Message.IMUAccZ, navigation.getAccel(Axis.Z));
            }

            debug.Write(false);
        }

        public Engine()
        {
            actualPosition = new Position();
            actualPosition.x = 0;
            actualPosition.y = 0;
        }

        public void Run()
        {
            while (true)
            {
                if (currentMode == Mode.SearchingForWall)
                {
                    coder.Send(Message.Mode, 0);
                    navigation.MoveToObject(sensors);

                    float central = sensors.getDistance(Sensor.Central);
                    float left = sensors.getDistance(Sensor.wall_back);
                    float right = sensors.getDistance(Sensor.Right);

                    if (central <= GlobalVal.distanceToDetect)
                    {
                        //WALL FOUNDED
                        navigation.resetDistance();
                        navigation.TurnUntilWall(sensors);
                        currentMode = Mode.FollowWall;
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
                        central = sensors.getDistance(Sensor.Central);

                        if (central < GlobalVal.distanceToDetect)
                        {
                            break;
                        }

                        if (exMath.Abs(wall - wallback) < GlobalVal.hysteresis)
                        {
                            if (wall > (GlobalVal.distanceToFollowWall + GlobalVal.hysteresis))
                            {
                                navigation.turnLeft();
                                Thread.Sleep(50);
                                continue;
                            }
                            else if (wall < (GlobalVal.distanceToFollowWall - GlobalVal.hysteresis))
                            {
                                navigation.turnRight();
                                Thread.Sleep(50);
                                continue;
                            }
                            else
                            {
                                navigation.MoveForward();
                            }
                        }
                        else if (wall < wallback)
                        {
                            navigation.turnRight();
                            Thread.Sleep(50);
                        }
                        else if (wall > wallback)
                        {
                            navigation.turnLeft();
                            Thread.Sleep(50);
                        }
                    }
                    navigation.TurnUntilWall(sensors);
                    continue;
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
