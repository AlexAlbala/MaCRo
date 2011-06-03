using System;
using Microsoft.SPOT;
using GHIElectronics.NETMF.FEZ;
using System.Threading;
using MaCRo.Drivers;
using MaCRo.Config;
using MaCRo.Tools;
using MaCRo.Drivers.IMU;

namespace MaCRo.Core
{
    public class NavigationManager
    {
        private Encoder left = new Encoder(PortMap.encoderL_interrupt);
        private Encoder right = new Encoder(PortMap.encoderR_interrupt);
        private DCMotorDriver dcm = new DCMotorDriver();
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

        public double distance_mm { get { return (left.distance_mm + right.distance_mm) / 2; } }

        public NavigationManager()
        {
            imu = new nIMU();
            //Thread.Sleep(GlobalVal.imuSettingTime);
            imu.Start();
            integrationTimeConstant = GlobalVal.integrationPeriod / 1e3;
            actualVelocity = new Position();
            actualPosition = new Position();

            lastTime = 0.0;

            //initialAcc = new double[3];
            initialHeading = this.MAG_Heading;

            //integration = new Timer(new TimerCallback(this.Integrate), new object(), GlobalVal.integrationPeriod * 10, GlobalVal.integrationPeriod);
        }

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

        public void TurnUntilWall(SensorManager sensors)
        {
            brake();
            turnRight(45);
            //Thread.Sleep(600);
            while (true)
            {
                float wall = sensors.getDistance(Sensor.Wall);
                float wall_back = sensors.getDistance(Sensor.wall_back);

                if (exMath.Abs(wall - wall_back) <= GlobalVal.hysteresis)
                    break;
                else
                    turnRight(5);
            }
            //brake();
        }

        public void MoveForward(int distancemm, sbyte speed)
        {
            this.resetDistance();

            MoveForward(speed);

            while (left.distance_mm < distancemm && right.distance_mm < distancemm)
            {
                Thread.Sleep(50);
            }

            this.brake();

            UpdatePosition();
        }

        public void MoveToObject(SensorManager sensors)
        {
            this.resetDistance();
            while (sensors.getDistance(Sensor.Central) > GlobalVal.distanceToDetect) { MoveForward(50, GlobalVal.speed); }
            this.brake();
        }

        private void UpdatePosition()
        {
            //angle respect of axis Y !!!
            actualPosition.x += (right.distance_mm + left.distance_mm) * exMath.Sin(actualPosition.angle) / 2000;
            actualPosition.y -= (right.distance_mm + left.distance_mm) * exMath.Cos(actualPosition.angle) / 2000;
        }

        public void MoveForward()
        {
            dcm.Move(GlobalVal.speed, GlobalVal.speed);
        }

        public void MoveForward(sbyte speed)
        {
            dcm.Move(speed, speed);
        }

        public void MoveBackward(int distancemm, sbyte speed)
        {
            MoveBackward(speed);

            while (left.distance_mm < distancemm && right.distance_mm < distancemm)
            {
                Thread.Sleep(100);
            }

            this.brake();
        }

        public void MoveBackward()
        {
            dcm.Move((sbyte)(GlobalVal.speed * -1), (sbyte)(GlobalVal.speed * -1));
        }

        public void MoveBackward(sbyte speed)
        {
            dcm.Move((sbyte)(speed * -1), (sbyte)(speed * -1));
        }

        public void turnRight(int angle)
        {
            double angleRad = exMath.ToRad(angle);

            //Let's suppose the mass center is in the geometrical center of the rover
            double lengthRight = exMath.Tan(angleRad) * GlobalVal.width_mm / 2;
            double lengthLeft = lengthRight;

            turnRight();

            while (right.distance_mm < lengthRight || left.distance_mm < lengthLeft)
            {
                Thread.Sleep(50);
            }
            this.brake();

            //actualPosition.angle += exMath.Atan2((left.distance_mm + right.distance_mm) / 2, GlobalVal.width_mm / 2);
            //actualPosition.angle = this.MAG_Heading - initialHeading;
            actualPosition.angle += (360 * (left.distance_mm + right.distance_mm) / 2) / (exMath.PI * GlobalVal.width_mm);
        }

        public void _turnRight()
        {
            this.resetDistance();
            dcm.Move(0, GlobalVal.turningSpeed);
        }

        public void turnRight()
        {
            this.resetDistance();
            dcm.Move((sbyte)(GlobalVal.turningSpeed * -1), GlobalVal.turningSpeed);
        }

        public void turnLeft(int angle)
        {
            double angleRad = exMath.ToRad(angle);

            //Let's suppose the mass center is in the geometrical center of the rover
            double lengthRight = exMath.Tan(angleRad) * GlobalVal.width_mm / 2;
            double lengthLeft = lengthRight;

            turnLeft();

            while (right.distance_mm < lengthRight || left.distance_mm < lengthLeft)
            {
                Thread.Sleep(50);
            }
            this.brake();

            //actualPosition.angle -= exMath.Atan2((left.distance_mm + right.distance_mm) / 2, GlobalVal.width_mm / 2);
            //actualPosition.angle = initialHeading - MAG_Heading;
            actualPosition.angle -= (360 * (left.distance_mm + right.distance_mm) / 2) / (exMath.PI * GlobalVal.width_mm);
        }

        public void _turnLeft()
        {
            this.resetDistance();
            dcm.Move(GlobalVal.turningSpeed, 0);
        }

        public void turnLeft()
        {
            this.resetDistance();
            dcm.Move(GlobalVal.turningSpeed, (sbyte)(GlobalVal.turningSpeed * -1));
        }

        public void brake()
        {
            dcm.Move(0, 0);
        }

        public void resetDistance()
        {
            left.resetDistance();
            right.resetDistance();
        }
    }
}
