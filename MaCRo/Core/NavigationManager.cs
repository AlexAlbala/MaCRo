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
        private nIMU imu = new nIMU();

        public double distance_mm { get { return (left.distance_mm + right.distance_mm) / 2; } }

        public short getAccel(Axis axis)
        {
            short[] accel = imu.getAccel();
            switch (axis)
            {
                case Axis.X:
                    return accel[0];
                case Axis.Y:
                    return accel[1];
                case Axis.Z:
                    return accel[2];
                default:
                    return 0;
            }
        }

        public short getGyro(Axis axis)
        {
            short[] gyro = imu.getGyro();
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

        public short getTemp(Axis axis)
        {
            short[] temp = imu.getTemp();
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

        public short getMag(Axis axis)
        {
            short[] mag = imu.getMag();
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
        public double lastTurnAngle
        {
            get
            {
                if (left.distance_mm < right.distance_mm)
                {
                    double angleRad = right.distance_mm / GlobalVal.width_mm;
                    double angle = angleRad * 180 / System.Math.PI;
                    return angle * -1;//TURN TO LEFT!
                }
                else
                {
                    double angleRad = left.distance_mm / GlobalVal.width_mm;
                    double angle = angleRad * 180 / System.Math.PI;
                    return angle;
                }
            }
        }

        public NavigationManager()
        {
            //imu.Start();
        }

        public void TurnUntilWall(SensorManager sensors)
        {
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
