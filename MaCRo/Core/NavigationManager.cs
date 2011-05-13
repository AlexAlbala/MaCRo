using System;
using Microsoft.SPOT;
using GHIElectronics.NETMF.FEZ;
using System.Threading;
using MaCRo.Drivers;
using MaCRo.Config;
using MaCRo.Tools;

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

        private double[] initialAcc;

        public double distance_mm { get { return (left.distance_mm + right.distance_mm) / 2; } }

        public NavigationManager()
        {
            imu = new nIMU();
            imu.Start();
            integrationTimeConstant = GlobalVal.integrationPeriod / 1e3;
            actualVelocity = new Position();
            actualPosition = new Position();
            
            lastTime = 0.0;

            initialAcc = new double[3];
            Calibrate();

            integration = new Timer(new TimerCallback(this.Integrate), new object(), 0, GlobalVal.integrationPeriod);
        }

        private void Calibrate()
        {
            Thread.Sleep(5 * GlobalVal.imuUpdate_ms);
            double[] temp = imu.getAccel();

            initialAcc[0] = temp[0];
            initialAcc[1] = temp[1];
            initialAcc[2] = temp[2];
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
                actualVelocity.x += ((9.807 * accX) * integrationTimeConstant);
                actualVelocity.y += ((9.807 * accY) * integrationTimeConstant);

                actualPosition.x += actualVelocity.x * integrationTimeConstant;
                actualPosition.y += actualVelocity.y * integrationTimeConstant;
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
                    return accel[0] - initialAcc[0];
                case Axis.Y:
                    return accel[1] - initialAcc[1];
                case Axis.Z:
                    return accel[2] - initialAcc[2];
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
            turnRight();
            Thread.Sleep(600);
            while (true)
            {
                float wall = sensors.getDistance(Sensor.Wall);
                float wall_back = sensors.getDistance(Sensor.wall_back);

                if (exMath.Abs(wall - wall_back) <= GlobalVal.hysteresis)
                    break;
            }
            brake();
        }

        public void MoveForward(int distancemm, sbyte speed)
        {
            MoveForward(speed);

            while (left.distance_mm < distancemm && right.distance_mm < distancemm)
            {
                Thread.Sleep(100);
            }

            this.brake();
        }

        public void MoveToObject(SensorManager sensors)
        {
            left.resetDistance();
            right.resetDistance();
            this.MoveForward();
            while (sensors.getDistance(Sensor.Central) > GlobalVal.distanceToDetect) { Thread.Sleep(50); }
            this.brake();
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
            double angleRad = angle * System.Math.PI / 180;
            double lengthLeft = angleRad * GlobalVal.width_mm;

            turnRight();

            while (left.distance_mm < lengthLeft)
            {
                Thread.Sleep(50);
            }
            this.brake();

        }

        public void turnRight()
        {
            left.resetDistance();
            right.resetDistance();
            dcm.Move(0, GlobalVal.turningSpeed);
        }

        public void _turnRight()
        {
            left.resetDistance();
            right.resetDistance();
            dcm.Move((sbyte)(GlobalVal.turningSpeed * -1), GlobalVal.turningSpeed);
        }

        public void turnLeft(int angle)
        {
            double angleRad = angle * System.Math.PI / 180;
            double lengthRight = angleRad * GlobalVal.width_mm;

            turnLeft();

            while (right.distance_mm < lengthRight)
            {
                Thread.Sleep(50);
            }
            this.brake();
        }

        public void turnLeft()
        {
            left.resetDistance();
            right.resetDistance();
            dcm.Move(GlobalVal.turningSpeed, 0);
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
