using System;
using Microsoft.SPOT;
using GHIElectronics.NETMF.FEZ;
using System.Threading;
using MaCRo.Drivers;
using MaCRo.Config;
using MaCRo.Tools;
//using MaCRo.Drivers.IMU;

namespace MaCRo.Core
{
    public class NavigationManager
    {
        private Encoder left = new Encoder(PortMap.encoderL_interrupt);
        private Encoder right = new Encoder(PortMap.encoderR_interrupt);
        private DCMotorDriver dcm = new DCMotorDriver();
        private Contingency contingency;
        private Magnetometer magnetometer = new Magnetometer();
        private bool manualStop;
        public Movement movement;
        private object monitor;
        private double initialHeading;
        private Position actualPosition;

        public sbyte manualSpeed { set; get; }
        public sbyte manualTurningSpeed { set; get; }

        public double distance_mm { get { return (left.distance_mm + right.distance_mm) / 2; } }

        public double MAG_Heading { get { return magnetometer.MAGHeadingRad; } }

        public double Relative_MAG_Heading { get { return MAG_Heading - initialHeading; } }

        #region IMU
        /*
        private nIMU imu;
        private Position actualPosition;
        private Position actualVelocity;
        private Timer integration;

        private double integrationTimeConstant;
        private double lastTime;
        private double initialHeading;


      

        //public double Yaw { get { return (double)exMath.ToDeg(imu.Yaw); } }
        //public double Pitch { get { return (double)exMath.ToDeg(imu.Roll); } }
        //public double Roll { get { return (double)exMath.ToDeg(imu.Pitch * -1); } }

        //-Y / X   (x,y) y/x
        public double MAG_Heading { get { return exMath.Atan2(getMag(Axis.X), -1 * getMag(Axis.Y)); } }

        public double Relative_MAG_Heading { get { return MAG_Heading - initialHeading; } }



        private void Integrate(Object state)
        {
            //METERS
            double accX = getAccel(Axis.X);
            double accY = getAccel(Axis.Y);
            double accZ = getAccel(Axis.Z);


            double actualTime = imu.getTime();
            if (lastTime != 0.0)
            {
                integrationTimeConstant = actualTime - lastTime;
            }

            lastTime = actualTime;


            lock (actualPosition)
            {
                actualVelocity.x += accX * integrationTimeConstant;
                actualVelocity.y += accY * integrationTimeConstant;

                actualPosition.x += actualVelocity.x * integrationTimeConstant;
                actualPosition.y += actualVelocity.y * integrationTimeConstant;

                //v_x = (a_x + pre_a_x) / 2 * sampling_rate + pre_v_x;

                //p_x = (v_x + pre_v_x) / 2 * sampling_rate + pre_p_x;

                //(a_x = acceleration, v_x = velocity, p_x = position, pre = previous)
            }
        }

        public Position getActualPosition()
        {
            return actualPosition;
        }

        public void setActualPosition(Position p)
        {
            this.actualPosition.x = p.x;
            this.actualPosition.y = p.y;
            this.actualPosition.angle = p.angle;
        }

        public Position getActualVelocity()
        {
            return actualVelocity;
        }

        public double getTime()
        {
            return lastTime;
        }

        public double getAccel(Axis axis)
        {
            double[] accel = imu.getAccel();
            switch (axis)
            {
                case Axis.X:
                    double accX = accel[0];// - initialAcc[0];
                    if (exMath.Abs(accX) < GlobalVal.accelerationThreshold)
                        accX = 0;
                    return accX;
                case Axis.Y:
                    double accY = accel[1];// - initialAcc[1];
                    if (exMath.Abs(accY) < GlobalVal.accelerationThreshold)
                        accY = 0;
                    return accY;
                case Axis.Z:
                    double accZ = accel[2];// - initialAcc[2];
                    if (exMath.Abs(accZ) < GlobalVal.accelerationThreshold)
                        accZ = 0;
                    return accZ;
                default:
                    return 0;
            }
        }

        public double getGyro(Axis axis)
        {
            double[] gyro = imu.getGyro();
            switch (axis)
            {
                case Axis.X:
                    return gyro[0];
                case Axis.Y:
                    return gyro[1];
                case Axis.Z:
                    return gyro[2];
                default:
                    return 0;
            }
        }

        public double getTemp(Axis axis)
        {
            double[] temp = imu.getTemp();
            switch (axis)
            {
                case Axis.X:
                    return temp[0];
                case Axis.Y:
                    return temp[1];
                case Axis.Z:
                    return temp[2];
                default:
                    return 0;
            }
        }

        public double getMag(Axis axis)
        {
            double[] mag = imu.getMag();
            switch (axis)
            {
                case Axis.X:
                    return mag[0];
                case Axis.Y:
                    return mag[1];
                case Axis.Z:
                    return mag[2];
                default:
                    return 0;
            }
        }
        */
        #endregion

        public NavigationManager()
        {
            movement = Movement.stop;
            /*
            imu = new nIMU();
            imu.Start();
            //integrationTimeConstant = GlobalVal.integrationPeriod / 1e3;
            actualVelocity = new Position();
            actualPosition = new Position();
            manualStop = false;
            monitor = new object();

            lastTime = 0.0;

            //initialAcc = new double[3];
            Thread.Sleep(1000);*/

            actualPosition = new Position();
            initialHeading = this.MAG_Heading;

            contingency = new Contingency(left, right, this);
            //integration = new Timer(new TimerCallback(this.Integrate), new object(), GlobalVal.integrationPeriod * 10, GlobalVal.integrationPeriod);
        }

        public Position getActualPosition()
        {
            return actualPosition;
        }

        public void setActualPosition(Position p)
        {
            this.actualPosition.x = p.x;
            this.actualPosition.y = p.y;
            this.actualPosition.angle = p.angle;
        }

        public void TurnRightUntilWall(SensorManager sensors)
        {
            brake();
            turnRight(45);
            //Thread.Sleep(600);
            while (!contingency.alarm)
            {
                float wall = sensors.getDistance(Sensor.Wall);
                float wall_back = sensors.getDistance(Sensor.wall_back);

                if (exMath.Abs(wall - wall_back) <= GlobalVal.hysteresis)
                    break;
                else
                    turnRight(5);
            }
        }

        public void TurnLeftUntilWall(SensorManager sensors)
        {
            brake();

            //MoveForward(100, GlobalVal.speed);
            turnLeft(45);

            while (sensors.getDistance(Sensor.Central) > GlobalVal.distanceToDetect)
            {
                float wall = sensors.getDistance(Sensor.Wall);
                float wall_back = sensors.getDistance(Sensor.wall_back);

                if (exMath.Abs(wall - wall_back) <= GlobalVal.hysteresis) { break; }
                else
                {
                    if (wall < wall_back)
                    {
                        turnRight(5);
                        MoveForward(10, GlobalVal.speed);
                        continue;
                    }
                    else
                    {
                        _turnLeft(10);
                        MoveForward(10, GlobalVal.speed);
                        continue;
                    }
                }
            }
        }

        public void MoveToObject(SensorManager sensors)
        {
            this.resetDistance();
            while (sensors.getDistance(Sensor.Central) > GlobalVal.distanceToDetect && !contingency.alarm) { MoveForward(50, GlobalVal.speed); }
            this.brake();
        }

        private void UpdatePosition(bool isBack)
        {
            double value = right.distance_mm + left.distance_mm;
            //angle respect of axis Y !!!
            if (isBack)
            {
                actualPosition.x -= value * exMath.Sin(actualPosition.angle) / 2000;
                actualPosition.y += value * exMath.Cos(actualPosition.angle) / 2000;
            }
            else
            {
                actualPosition.x += value * exMath.Sin(actualPosition.angle) / 2000;
                actualPosition.y -= value * exMath.Cos(actualPosition.angle) / 2000;
            }
            this.resetDistance();
        }

        public void MoveForward(int distancemm)
        {
            this.resetDistance();

            MoveForward();

            while ((left.distance_mm + right.distance_mm) < distancemm * 2 && !contingency.alarm) { Thread.Sleep(50); }

            this.brake();

            UpdatePosition(false);
        }

        public void MoveForward(int distancemm, sbyte speed)
        {
            this.resetDistance();

            MoveForward(speed);

            while ((left.distance_mm + right.distance_mm) < distancemm * 2 && !contingency.alarm) { Thread.Sleep(50); }

            this.brake();

            UpdatePosition(false);
        }

        private void MoveForward()
        {
            movement = Movement.forward;
            dcm.Move((sbyte)(GlobalVal.speed + 2), (sbyte)(GlobalVal.speed - 1));
        }

        private void MoveForward(sbyte speed)
        {
            movement = Movement.forward;
            dcm.Move((sbyte)(speed + 2), (sbyte)(speed - 1));
        }

        public void MoveBackward(int distancemm)
        {
            this.resetDistance();
            MoveBackward();

            while (left.distance_mm < distancemm && right.distance_mm < distancemm && !contingency.alarm)
            {
                Thread.Sleep(100);
            }

            this.brake();
            UpdatePosition(true);
        }

        public void MoveBackward(int distancemm, sbyte speed)
        {
            this.resetDistance();
            MoveBackward(speed);

            while (left.distance_mm < distancemm && right.distance_mm < distancemm && !contingency.alarm)
            {
                Thread.Sleep(100);
            }

            this.brake();
            UpdatePosition(true);
        }

        private void MoveBackward()
        {
            movement = Movement.backward;
            dcm.Move((sbyte)(GlobalVal.speed * -1), (sbyte)(GlobalVal.speed * -1));
        }

        private void MoveBackward(sbyte speed)
        {
            movement = Movement.backward;
            dcm.Move((sbyte)(speed * -1), (sbyte)(speed * -1));
        }

        public void turnRight(int angle)
        {
            double angleRad = exMath.ToRad(angle);

            //Let's suppose the mass center is in the geometrical center of the rover
            double lengthRight = angleRad * GlobalVal.width_correction * GlobalVal.distanceBetweenWheels_mm / 2;
            double lengthLeft = lengthRight;

            turnRight();

            while (right.distance_mm < lengthRight && left.distance_mm < lengthLeft && !contingency.alarm)
            {
                Thread.Sleep(50);
            }
            this.brake();

            actualPosition.angle += (left.distance_mm + right.distance_mm) / (GlobalVal.width_correction * GlobalVal.distanceBetweenWheels_mm);
            //actualPosition.angle = this.Relative_MAG_Heading;
        }

        public void turnRight(int angle, sbyte speed)
        {
            double angleRad = exMath.ToRad(angle);

            //Let's suppose the mass center is in the geometrical center of the rover
            double lengthRight = angleRad * GlobalVal.width_correction * GlobalVal.distanceBetweenWheels_mm / 2;
            double lengthLeft = lengthRight;

            turnRight(speed);

            while (right.distance_mm < lengthRight && left.distance_mm < lengthLeft && !contingency.alarm)
            {
                Thread.Sleep(50);
            }
            this.brake();

            actualPosition.angle += (left.distance_mm + right.distance_mm) / (GlobalVal.width_correction * GlobalVal.distanceBetweenWheels_mm);
            //actualPosition.angle = this.Relative_MAG_Heading;
        }

        private void turnRight()
        {
            movement = Movement.right;
            this.resetDistance();
            dcm.Move((sbyte)(GlobalVal.turningSpeed * -1), GlobalVal.turningSpeed);
        }

        private void turnRight(sbyte speed)
        {
            movement = Movement.right;
            this.resetDistance();
            dcm.Move((sbyte)(speed * -1), speed);
        }

        public void turnLeft(int angle)
        {
            double angleRad = exMath.ToRad(angle);

            //Let's suppose the mass center is in the geometrical center of the rover
            double lengthRight = angleRad * GlobalVal.width_correction * GlobalVal.distanceBetweenWheels_mm / 2;
            double lengthLeft = lengthRight;

            turnLeft();

            while (right.distance_mm < lengthRight && left.distance_mm < lengthLeft && !contingency.alarm)
            {
                Thread.Sleep(50);
            }
            this.brake();

            actualPosition.angle -= (left.distance_mm + right.distance_mm) / (GlobalVal.width_correction * GlobalVal.distanceBetweenWheels_mm);
            //actualPosition.angle = this.Relative_MAG_Heading;
        }

        public void turnLeft(int angle, sbyte speed)
        {
            double angleRad = exMath.ToRad(angle);

            //Let's suppose the mass center is in the geometrical center of the rover
            double lengthRight = angleRad * GlobalVal.width_correction * GlobalVal.distanceBetweenWheels_mm / 2;
            double lengthLeft = lengthRight;

            turnLeft(speed);

            while (right.distance_mm < lengthRight && left.distance_mm < lengthLeft && !contingency.alarm)
            {
                Thread.Sleep(50);
            }
            this.brake();

            actualPosition.angle -= (left.distance_mm + right.distance_mm) / (GlobalVal.width_correction * GlobalVal.distanceBetweenWheels_mm);
            //actualPosition.angle = this.Relative_MAG_Heading;
        }

        private void turnLeft()
        {
            movement = Movement.left;
            this.resetDistance();
            dcm.Move(GlobalVal.turningSpeed, (sbyte)(GlobalVal.turningSpeed * -1));
        }

        private void turnLeft(sbyte speed)
        {
            movement = Movement.left;
            this.resetDistance();
            dcm.Move(speed, (sbyte)(speed * -1));
        }

        #region NOT USED
        public void _turnRight(int angle)
        {
            double angleRad = exMath.ToRad(angle);

            //Let's suppose the mass center is in the geometrical center of the rover
            double lengthRight = angleRad * GlobalVal.distanceBetweenWheels_mm;
            double lengthLeft = lengthRight;

            _turnRight();

            while (right.distance_mm < lengthRight && left.distance_mm < lengthLeft && !contingency.alarm)
            {
                Thread.Sleep(50);
            }
            this.brake();

            //actualPosition.angle += exMath.Atan2((left.distance_mm + right.distance_mm) / 2, GlobalVal.width_mm / 2);
            //actualPosition.angle = this.MAG_Heading - initialHeading;
            actualPosition.angle += (left.distance_mm) / GlobalVal.distanceBetweenWheels_mm;
        }

        public void _turnRight()
        {
            movement = Movement.right;
            this.resetDistance();
            dcm.Move(0, GlobalVal.turningSpeed);
        }

        public void _turnLeft()
        {
            movement = Movement.left;
            this.resetDistance();
            dcm.Move(GlobalVal.turningSpeed, 0);
        }

        public void _turnLeft(int angle)
        {
            double angleRad = exMath.ToRad(angle);

            double lengthRight = angleRad * GlobalVal.distanceBetweenWheels_mm;


            _turnLeft();

            while (right.distance_mm < lengthRight)
            {
                Thread.Sleep(50);
            }
            this.brake();

            //actualPosition.angle -= exMath.Atan2((left.distance_mm + right.distance_mm) / 2, GlobalVal.width_mm / 2);
            //actualPosition.angle = initialHeading - MAG_Heading;
            actualPosition.angle -= (left.distance_mm + right.distance_mm) / 2 * GlobalVal.distanceBetweenWheels_mm;
        }
        #endregion

        public void brake()
        {
            movement = Movement.stop;
            dcm.Move(0, 0);
        }

        public void resetDistance()
        {
            left.resetDistance();
            right.resetDistance();
        }

        public void disableContingency()
        {
            contingency.Disable();
        }

        public void restartContingency()
        {
            contingency.Restart();
        }

        #region MANUAL CONTROLS
        public void ManualForward()
        {
            lock (monitor)
            {
                manualStop = false;
                while (!manualStop)
                {
                    MoveForward(10, this.manualSpeed);
                }
                this.brake();
            }
        }

        public void ManualBackward()
        {
            lock (monitor)
            {
                manualStop = false;
                while (!manualStop)
                {
                    MoveBackward(10, this.manualSpeed);
                }
                this.brake();
            }
        }

        public void ManualLeft()
        {
            lock (monitor)
            {
                manualStop = false;
                while (!manualStop)
                {
                    turnLeft(5, this.manualTurningSpeed);
                }
                this.brake();
            }
        }

        public void ManualRight()
        {
            lock (monitor)
            {
                manualStop = false;
                while (!manualStop)
                {
                    turnRight(5, this.manualTurningSpeed);
                }
                this.brake();
            }
        }

        public void ManualBrake()
        {
            manualStop = true;
        }

        #endregion
    }
}
