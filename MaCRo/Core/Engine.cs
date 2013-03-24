using System;
using Microsoft.SPOT;
using System.Threading;
using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT.Hardware;
using MaCRo.Config;
using MaCRo.Tools;
using MaCRo.Communications;
using MaCRo.Drivers;
using MaCRo.Core;


namespace MaCRo.Core
{
    class Engine
    {
        private static Engine instance;

        private OutputPort debug;

        private NavigationManager navigation;
        //private Magnetometer magnetometer;
        private SensorManager sensors;
        private BatteryManager battery;
        private Coder coder;
        private bool cancel;

        private Mode currentMode;

        private Position actualPosition;

        private Thread workerThread;
        private Timer sensorTimer;
        private Timer positionTimer;
        private Timer batteryTimer;
        private Timer magTimer;

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
            Thread.Sleep(17000);
            coder.Start();

            Info("Initializing System...");

            navigation = new NavigationManager();
            Info("Initialized NavigationManager");
            sensors = new SensorManager();
            Info("Initialized SensorManager");
            battery = new BatteryManager();
            Info("Initialized BatteryManager");

            currentMode = Mode.SearchingForWall;

            sensorTimer = new Timer(new TimerCallback(sensorTimer_tick), new object(), 0, GlobalVal.transmissionPeriodSensor_ms);
            positionTimer = new Timer(new TimerCallback(posTimer_Tick), new object(), 0, GlobalVal.transmissionPeriodPosition_ms);
            batteryTimer = new Timer(new TimerCallback(batteryTimer_Tick), new object(), 0, GlobalVal.transmissionPeriodBattery_ms);
            magTimer = new Timer(new TimerCallback(magnetometer_Tick), new object(), 0, GlobalVal.trasmissionPeriodMagnetometer_ms);

            Info("Initialized Timers");
            

            cancel = false;
            debug.Write(false);

            navigation.manualSpeed = GlobalVal.speed;
            navigation.manualTurningSpeed = GlobalVal.turningSpeed;
            Info("Ready");
        }

        void batteryTimer_Tick(Object state)
        {
            double current = battery.getBatteryCurrent();
            if (current > 0)
                coder.Send(Message.Current, current);

            coder.Send(Message.Voltage, battery.getBatteryVoltage());
            coder.Send(Message.Capacity, battery.getBatteryCapacity());

            ushort estimation = battery.getBatteryEstimation_minutes();
            if (estimation > 0)
                coder.Send(Message.Estimation, estimation);

            if (battery.lowBattery)//TODO
            {
                //this.Cancel();
                Info("LOW BATTERY!!: " + battery.getBatteryCapacity() + " %. System stopped for saving battery.");
                //sensorTimer = null;
                //positionTimer = null;
                //batteryTimer = null;
                //magTimer = null;
            }
        }

        

        void posTimer_Tick(Object state)
        {
            lock (coder)
            {
                Position pos = navigation.getActualPosition();

                coder.Send(Message.PositionX, pos.x);
                coder.Send(Message.PositionY, pos.y);
                coder.Send(Message.Angle, pos.angle);

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
                short value;
                if (s1 > 300)
                {
                    value = -1;
                }
                else if (s1 < 40)
                {
                    value = -2;
                }
                else
                {
                    value = s1;
                }
                coder.Send(Message.SensorS1, value);

                ;
                if (s2 > 300)
                {
                    value = -1;
                }
                else if (s2 < 40)
                {
                    value = -2;
                }
                else
                {
                    value = s2;
                }
                coder.Send(Message.SensorS2, value);

                if (l1 > 800)
                {
                    value = -1;
                }
                else if (l1 < 100)
                {
                    value = -2;
                }
                else
                {
                    value = l1;
                }
                coder.Send(Message.SensorL1, value);

                if (l2 > 800)
                {
                    value = -1;
                }
                else if (l2 < 100)
                {
                    value = -2;
                }
                else
                {
                    value = l2;
                }
                coder.Send(Message.SensorL2, value);


            }
        }

        void magnetometer_Tick(Object state)
        {
            lock (coder)
            {
                coder.Send(Message.MagX, navigation.getMag(Axis.X));
                coder.Send(Message.MagY, navigation.getMag(Axis.Y));
                coder.Send(Message.MagZ, navigation.getMag(Axis.Z));
            }
        }

        public void calibrarX(short cx)
        {
            Info("X=" + cx);
            navigation.SetXOFF(cx);
        }
        public void calibrarY(short cy)
        {
            Info("Y=" + cy);
            navigation.SetYOFF(cy);
        }
        /*public short calibrarZ(short cz)
         {
           return cz;
         }*/

        private Engine()
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
            navigation.brake();

        }

        public void Info(string message)
        {
            coder.Send(Message.Info, message);
        }

        public void Debug(string message)
        {
            coder.Send(Message.Debug, message);
        }

        public void Error(string message)
        {
            coder.Send(Message.Error, message);
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
            navigation.manualSpeed = (sbyte)speed;
        }

        public void ManualTurningSpeed(short speed)
        {
            navigation.manualTurningSpeed = (sbyte)speed;
        }

        public void ManualMode()
        {
            Info("Starting manual mode");
            this.Cancel();
            this.currentMode = Mode.Manual;
            navigation.resetDistance();
            navigation.disableContingency();
            Info("Started manual mode");
            debug.Write(true);
        }

        public void StopManualMode()
        {
            if (currentMode == Mode.Manual)
            {
                this.currentMode = Mode.SearchingForWall;
                //this.Restart();
                navigation.restartContingency();
                Info("Stopped manual mode");
                Info("Current mode: Searching a wall");
            }
        }

        public void ManualForward(short speed)
        {
            if (currentMode == Mode.Manual && navigation.movement == Movement.stop)
            {
                Thread t = new Thread(new ThreadStart(navigation.ManualForward));
                t.Start();
            }
        }

        public void ManualBackward(short speed)
        {
            if (currentMode == Mode.Manual && navigation.movement == Movement.stop)
            {
                Thread t = new Thread(new ThreadStart(navigation.ManualBackward));
                t.Start();
            }
        }

        public void ManualRight()
        {
            if (currentMode == Mode.Manual && navigation.movement == Movement.stop)
            {
                Thread t = new Thread(new ThreadStart(navigation.ManualRight));
                t.Start();
            }
        }

        public void ManualLeft()
        {
            if (currentMode == Mode.Manual && navigation.movement == Movement.stop)
            {
                Thread t = new Thread(new ThreadStart(navigation.ManualLeft));
                t.Start();
            }
        }

        public void ManualStop()
        {
            if (currentMode == Mode.Manual)
            {
                Thread t = new Thread(new ThreadStart(navigation.ManualBrake));
                t.Start();
            }
        }
        #endregion

        public void UpdatePosition(Position p)
        {
            Position _p = navigation.getActualPosition();
            Debug("Position: " + _p.x + " : " + _p.y + " : " + _p.angle + " | " + p.x + "  :" + p.y + " : " + p.angle);
            navigation.setActualPosition(p);
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
                    navigation.MoveToObject(sensors);

                    float central = sensors.getDistance(Sensor.Central);

                    if (central <= GlobalVal.distanceToDetect)
                    {
                        //WALL FOUND
                        navigation.TurnRightUntilWall(sensors);
                        currentMode = Mode.FollowWall;
                        Info("Current mode: Follow Wall");
                    }
                    continue;
                }
                else if (currentMode == Mode.FollowWall)
                {
                    float central;
                    float wall;
                    float wallback;
                    double error_distance, angle_error;

                    //FOLLOW LEFT WALL
                    while (!cancel)
                    {
                        wall = sensors.getDistance(Sensor.Wall);
                        wallback = sensors.getDistance(Sensor.wall_back);
                        central = sensors.getDistance(Sensor.Central);
                        error_distance = 0;
                        angle_error = 0;

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

                                navigation.MoveForward(20);
                            }
                            else
                            {
                                continue;
                            }
                        }//CORRECT THE DEVIATION RESPECT TO THE WALL
                        else if (wall < wallback)
                        {
                            error_distance = wall - wallback;

                            if (error_distance < 5)
                            {
                                angle_error = exMath.Atan2(error_distance * 10, GlobalVal.length_mm);
                                navigation.turnLeft((int)(angle_error / 2));
                                navigation.MoveForward(30);//50
                            }
                            else//THERE IS A CORNER
                            {
                                Debug("LEFT CORNER");
                                navigation.TurnLeftUntilWall(sensors);
                            }
                        }//IN THE FOLLOWING CASE:
                        //1-IS A WALL DEVIATION
                        //2-THERE IS A CORNER
                        else if (wall > wallback)
                        {
                            if ((wall - wallback) < 5)
                            {
                                navigation.turnLeft(1);
                                navigation.MoveForward(50);
                            }
                            else//THERE IS A CORNER
                            {
                                Debug("LEFT CORNER");
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
