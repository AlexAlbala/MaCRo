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
        private bool cancel;

        private Mode currentMode;

        private Position actualPosition;

        private Thread workerThread;
        private Timer sensorTimer;
        private Timer positionTimer;
        private Timer IMUTimer;
        private Timer TempTimer;

        public static Engine getInstance()
        {
            if (instance == null)
                instance = new Engine();
            return instance;
        }

        public void InitializeSystem()
        {
            debug = new OutputPort((Cpu.Pin)PortMap.debug, false);
            debug.Write(true);

            navigation = new NavigationManager();
            Thread.Sleep(2000);

            sensors = new SensorManager();
            coder = new Coder();
            coder.Start();

            currentMode = Mode.SearchingForWall;

            sensorTimer = new Timer(new TimerCallback(sensorTimer_tick), new object(), 0, GlobalVal.transmissionPeriodSensor_ms);
            positionTimer = new Timer(new TimerCallback(posTimer_Tick), new object(), 0, GlobalVal.transmissionPeriodPosition_ms);
            IMUTimer = new Timer(new TimerCallback(imuTimer_Tick), new object(), 0, GlobalVal.transmissionPeriodIMU_ms);
            TempTimer = new Timer(new TimerCallback(tempTimer_Tick), new object(), 0, GlobalVal.transmissionPeriodTemp_ms);

            cancel = false;

            debug.Write(false);
        }

        void imuTimer_Tick(Object state)
        {
            //IMU Telemetry
            coder.Send(Message.IMUMagX, navigation.getMag(Axis.X));
            coder.Send(Message.IMUMagY, navigation.getMag(Axis.Y));
            coder.Send(Message.IMUMagZ, navigation.getMag(Axis.Z));

            coder.Send(Message.IMUGyrX, navigation.getGyro(Axis.X));
            coder.Send(Message.IMUGyrY, navigation.getGyro(Axis.Y));
            coder.Send(Message.IMUGyrZ, navigation.getGyro(Axis.Z));

            coder.Send(Message.IMUAccX, navigation.getAccel(Axis.X));
            coder.Send(Message.IMUAccY, navigation.getAccel(Axis.Y));
            coder.Send(Message.IMUAccZ, navigation.getAccel(Axis.Z));
        }

        void tempTimer_Tick(Object state)
        {
            coder.Send(Message.IMUTempX, navigation.getTemp(Axis.X));
            coder.Send(Message.IMUTempY, navigation.getTemp(Axis.Y));
            coder.Send(Message.IMUTempZ, navigation.getTemp(Axis.Z));
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

                //coder.Send(Message.Pitch, navigation.Pitch);
                //coder.Send(Message.Roll, navigation.Roll);
                //coder.Send(Message.Yaw, navigation.Yaw);
                coder.Send(Message.MAGHeading, (double)exMath.ToDeg((float)navigation.MAG_Heading));

            }
        }

        void sensorTimer_tick(Object state)
        {
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
            }
        }

        public Engine()
        {
            actualPosition = new Position();
        }

        public void Restart()
        {
            cancel = false;
            this.Run();
        }

        public void Cancel()
        {
            cancel = true;
            workerThread.Abort();
            //Thread.Sleep(500);
            navigation.brake();


            //Thread.Sleep(1000);
        }

        public void Run()
        {
            workerThread = new Thread(new ThreadStart(_Run));
            workerThread.Start();
        }

        private void _Run()
        {
            while (!cancel)
            {
                debug.Write(true);
                Thread.Sleep(50);
                debug.Write(false);

                if (currentMode == Mode.SearchingForWall)
                {
                    coder.Send(Message.Mode, 0);
                    navigation.MoveToObject(sensors);

                    float central = sensors.getDistance(Sensor.Central);

                    if (central <= GlobalVal.distanceToDetect)
                    {
                        //WALL FOUND
                        navigation.TurnUntilWall(sensors);
                        currentMode = Mode.FollowWall;
                    }
                    continue;
                }
                else if (currentMode == Mode.FollowWall)
                {
                    coder.Send(Message.Mode, 1);
                    float central;
                    float wall;
                    float wallback;

                    //FOLLOW LEFT WALL
                    while (!cancel)
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
                            if (wall < 30 && wallback < 30)
                            {
                                //Maintain certain distance to the wall (too close may be a problem)
                                if (wall < GlobalVal.minDistanceToFollowWall || wallback < GlobalVal.minDistanceToFollowWall)
                                    navigation.turnRight(10);

                                navigation.MoveForward(50, GlobalVal.speed);
                            }
                            else
                            {
                                continue;
                            }
                        }//CORRECT THE DEVIATION RESPECT TO THE WALL
                        else if (wall < wallback)
                        {
                            if (wall < 6)
                            {
                                navigation.turnRight(10);
                            }
                            else
                            {
                                navigation.turnRight(1);
                            }

                            navigation.MoveForward(50, GlobalVal.speed);
                        }//IN THE FOLLOWING CASE:
                        //1-IS A WALL DEVIATION
                        //2-THERE IS A CORNER
                        else if (wall > wallback)
                        {
                            if ((wall - wallback) < 5)
                            {
                                navigation.turnLeft(1);
                                navigation.MoveForward(50, GlobalVal.speed);
                            }
                            else//THERE IS A CORNER
                            {
                                navigation.TurnLeftUntilWall(sensors);
                            }
                        }
                    }
                    if (!cancel)
                        navigation.TurnUntilWall(sensors);
                }

            }

        }

    }
}
