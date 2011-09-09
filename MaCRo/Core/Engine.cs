using System;
using Microsoft.SPOT;
using System.Threading;
using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT.Hardware;
using MaCRo.Config;
using MaCRo.Tools;
using MaCRo.Communications;
using MaCRo.Core.SLAM;

namespace MaCRo.Core
{
    class Engine
    {
        private static Engine instance;

        private OutputPort debug;

        private NavigationManager navigation;
        private SensorManager sensors;
        private Coder coder;
        private bool cancel;

        private Mode currentMode;

        private Position actualPosition;

        private Thread workerThread;
        private Timer sensorTimer;
        private Timer positionTimer;


        //private Timer IMUTimer;
        //private Timer TempTimer;
        //private Timer mapTimer;
        //private Timer mapTransmissionTimer;

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

            coder = new Coder();
            coder.Start();

            coder.Send(Message.Info, "Initializing System...");

            navigation = new NavigationManager();

            sensors = new SensorManager();

            currentMode = Mode.SearchingForWall;

            sensorTimer = new Timer(new TimerCallback(sensorTimer_tick), new object(), 0, GlobalVal.transmissionPeriodSensor_ms);
            positionTimer = new Timer(new TimerCallback(posTimer_Tick), new object(), 0, GlobalVal.transmissionPeriodPosition_ms);
            //IMUTimer = new Timer(new TimerCallback(imuTimer_Tick), new object(), 0, GlobalVal.transmissionPeriodIMU_ms);
            //TempTimer = new Timer(new TimerCallback(tempTimer_Tick), new object(), 0, GlobalVal.transmissionPeriodTemp_ms);

            /*******SLAM**************/

            //mapTransmissionTimer = new Timer(new TimerCallback(mapTransmission_Tick), new object(), GlobalVal.mapUpdatePeriod_ms * 5, GlobalVal.mapTransmissionPeriod_ms);
            //mapTimer = new Timer(new TimerCallback(mapScan_tick), new object(), 0, GlobalVal.mapUpdatePeriod_ms);

            //tinySLAM.Initialize();
            //coder.Send(Message.MapSize, tinySLAM.MapSize());

            /*******SLAM**************/

            cancel = false;

            debug.Write(false);

            navigation.manualSpeed = GlobalVal.speed;
            navigation.manualTurningSpeed = GlobalVal.turningSpeed;
            coder.Send(Message.Info, "Ready");
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

                //coder.Send(Message.Time, navigation.getTime());

                //coder.Send(Message.Pitch, navigation.Pitch);
                //coder.Send(Message.Roll, navigation.Roll);
                //coder.Send(Message.Yaw, navigation.Yaw);
                //coder.Send(Message.MAGHeading, (double)exMath.ToDeg((float)navigation.MAG_Heading));

                //Debug.Print("MAG:" + (double)exMath.ToDeg((float)navigation.MAG_Heading));
                //Debug.Print("GYR: " + navigation.getGyro(Axis.X).ToString() + "X");
                //Debug.Print("ACC: " + navigation.getAccel(Axis.X).ToString() + "X");

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
                else
                    coder.Send(Message.SensorS1, (short)-1);
                if (s2 > 40 && s2 < 300)
                    coder.Send(Message.SensorS2, s2);
                else
                    coder.Send(Message.SensorS2, (short)-1);
                if (l1 > 100 && l1 < 800)
                    coder.Send(Message.SensorL1, l1);
                else
                    coder.Send(Message.SensorL1, (short)-1);
                if (l2 > 100 && l2 < 800)
                    coder.Send(Message.SensorL2, l2);
                else
                    coder.Send(Message.SensorL2, (short)-1);
            }
        }

        /*
        #region SLAM
        private void mapTransmission_Tick(object state)
        {
            ushort[] map = tinySLAM.Map().map;
            int length = map.Length;

            int l = length / 4;
            int l4 = l + length % 4;
            ushort[] m1, m2, m3, m4;

            m1 = new ushort[l];
            m2 = new ushort[l];
            m3 = new ushort[l];
            m4 = new ushort[l4];
            for (int i = 0; i < l; i++)
            {
                m1[i] = map[i];
                m2[i] = map[l + i];
                m3[i] = map[2 * l + i];
                m4[i] = map[3 * l + i];
            }
            for (int i = l; i < l4; i++)
            {
                m4[i] = map[3 * l + i];
            }
            map = null;

            coder.Send(Message.MapUpdate1, m1);
            m1 = null;
            coder.Send(Message.MapUpdate2, m2);
            m2 = null;
            coder.Send(Message.MapUpdate3, m3);
            m3 = null;
            coder.Send(Message.MapUpdate4, m4);
            m4 = null;
        }

        private void mapScan_tick(Object state)
        {
            float wall = 10 * sensors.getDistance(Sensor.Wall);
            float wall_back = 10 * sensors.getDistance(Sensor.wall_back);
            float central = 10 * sensors.getDistance(Sensor.Central);
            float right = 10 * sensors.getDistance(Sensor.Right);

            ts_scan_t scan = new ts_scan_t();

            int j = 0;
            int angle = 0;
            //CENTRAL SENSOR:
            angle -= 15;
            for (int i = 0; i < 30; angle++, i++, j++)
            {
                float angleRad = exMath.ToRad(angle);
                float c = (float)exMath.Cos(angleRad);
                float s = (float)exMath.Sin(angleRad);

                scan.x[j] = 0 - central * s;
                scan.y[j] = central * c + GlobalVal.length_mm / 2;
                scan.value[j] = SLAMAlgorithm.TS_OBSTACLE;
            }


            //WALL SENSOR
            angle = 270;
            angle -= 15;
            for (int i = 0; i < 30; angle++, i++, j++)
            {
                float angleRad = exMath.ToRad(angle);
                float c = (float)exMath.Cos(angleRad);
                float s = (float)exMath.Sin(angleRad);

                scan.x[j] = -GlobalVal.width_mm / 2 - wall * c;
                scan.y[j] = central * s + GlobalVal.length_mm / 2;
                scan.value[j] = SLAMAlgorithm.TS_OBSTACLE;
            }
            scan.nb_points = (ushort)j;

            ts_position_t position = new ts_position_t();
            Position p = navigation.getActualPosition();

            position.theta = exMath.ToDeg((float)p.angle);
            position.x = p.x + SLAMAlgorithm.TS_MAP_SIZE / (SLAMAlgorithm.TS_MAP_SCALE * 2);
            position.y = p.y + SLAMAlgorithm.TS_MAP_SIZE / (SLAMAlgorithm.TS_MAP_SCALE * 2);
            tinySLAM.NewScan(scan, position);
        }

        #endregion
         */

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
            if (workerThread != null)
            {
                if (workerThread.IsAlive)
                {
                    workerThread.Abort();
                }
            }
            workerThread = new Thread(new ThreadStart(_Run));
            workerThread.Start();
        }

        #region ManualMode

        public void ManualSpeed(short speed)
        {
            navigation.manualSpeed = speed;
        }

        public void ManualTurningSpeed(short speed)
        {
            navigation.manualTurningSpeed = speed;
        }

        public void ManualMode()
        {
            coder.Send(Message.Info, "Starting manual mode");
            this.Cancel();
            this.currentMode = Mode.Manual;
            navigation.resetDistance();
            navigation.disableContingency();
            coder.Send(Message.Info, "Started manual mode");
        }

        public void StopManualMode()
        {
            if (currentMode == Mode.Manual)
            {
                this.currentMode = Mode.SearchingForWall;
                this.Restart();
                navigation.restartContingency();
                coder.Send(Message.Info, "Stopped manual mode");
                coder.Send(Message.Info, "Current mode: Searching a wall");
            }
        }

        public void ManualForward(short speed)
        {
            if (currentMode == Mode.Manual)
            {
                Thread t = new Thread(new ThreadStart(navigation.ManualForward));
                t.Start();
            }
        }

        public void ManualBackward(short speed)
        {
            if (currentMode == Mode.Manual)
            {
                Thread t = new Thread(new ThreadStart(navigation.ManualBackward));
                t.Start();
            }
        }

        public void ManualRight()
        {
            if (currentMode == Mode.Manual)
            {
                Thread t = new Thread(new ThreadStart(navigation.ManualRight));
                t.Start();
            }
        }

        public void ManualLeft()
        {
            if (currentMode == Mode.Manual)
            {
                Thread t = new Thread(new ThreadStart(navigation.ManualLeft));
                t.Start();
            }
        }

        public void ManualStop()
        {
            if (currentMode == Mode.Manual)
            {
                Thread t = new Thread(new ThreadStart(navigation.manualBrake));
                t.Start();
            }
        }
        #endregion

        private void _Run()
        {
            coder.Send(Message.Debug, "_Run method started");
            /****************** TEST CODE *********************/
            //navigation.MoveForward(100, GlobalVal.speed);
            //navigation.turnRight(90);
            //navigation.MoveForward(100, GlobalVal.speed);
            //navigation.turnRight(90);
            //navigation.MoveForward(100, GlobalVal.speed);
            //navigation.turnRight(90);
            //navigation.MoveForward(100, GlobalVal.speed);
            //return;
            /****************** TEST CODE *********************/
            while (!cancel)
            {
                debug.Write(true);
                Thread.Sleep(50);
                debug.Write(false);

                if (currentMode == Mode.SearchingForWall)
                {
                    navigation.MoveToObject(sensors);

                    float central = sensors.getDistance(Sensor.Central);

                    if (central <= GlobalVal.distanceToDetect)
                    {
                        //WALL FOUND
                        navigation.TurnRightUntilWall(sensors);
                        currentMode = Mode.FollowWall;
                        coder.Send(Message.Info, "Current mode: Follow Wall");
                    }
                    continue;
                }
                else if (currentMode == Mode.FollowWall)
                {
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
                                {
                                    if (wall <= wallback)
                                        navigation.turnRight(10);
                                }

                                navigation.MoveForward(30, GlobalVal.speed);
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
                                coder.Send(Message.Debug, "LEFT CORNER");
                                navigation.TurnLeftUntilWall(sensors);
                            }
                        }
                    }
                    if (!cancel)
                        navigation.TurnRightUntilWall(sensors);
                }

            }

        }

    }
}
