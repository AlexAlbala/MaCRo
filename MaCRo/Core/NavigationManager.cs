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
        private Orientation actualOrientation;

        public double distance_mm { get { return (left.distance_mm + right.distance_mm) / 2; } }

        public NavigationManager()
        {
            actualOrientation = Orientation.HorizontalPos;
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
            while (sensors.getDistance(Sensor.Central) > 5) {}
            this.brake();
        }

        public void MoveForward()
        {
            left.resetDistance();
            right.resetDistance();
            dcm.Move(GlobalVal.speed, GlobalVal.speed);
        }

        public void MoveForward(sbyte speed)
        {
            left.resetDistance();
            right.resetDistance();
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
            left.resetDistance();
            right.resetDistance();
            dcm.Move((sbyte)(GlobalVal.speed * -1), (sbyte)(GlobalVal.speed * -1));
        }

        public void MoveBackward(sbyte speed)
        {
            left.resetDistance();
            right.resetDistance();
            dcm.Move((sbyte)(speed * -1), (sbyte)(speed * -1));
        }

        public void turnRight(int angle)
        {
            double angleRad = angle * System.Math.PI / 180;
            double lengthLeft = angleRad * ((GlobalVal.width_mm) + GlobalVal.turnRadius);

            turnRight();

            while (left.distance_mm < lengthLeft)
            {
                Thread.Sleep(300);
            }
            this.brake();

        }

        public void turnRight()
        {
            left.resetDistance();
            right.resetDistance();
            dcm.Move(0, GlobalVal.turningSpeed);
        }

        public void turnLeft(int angle)
        {
            double angleRad = angle * System.Math.PI / 180;
            double lengthRight = 1.01 * angleRad * GlobalVal.width_mm;

            turnLeft();

            while (right.distance_mm < lengthRight)
            {
                Thread.Sleep(300);
            }
            this.brake();
        }

        public void turnLeft()
        {
            left.resetDistance();
            right.resetDistance();
            dcm.Move(GlobalVal.turningSpeed,0);
        }

        public void brake()
        {
            dcm.Move(0, 0);
        }

        public void orientationToRight()
        {
            switch (actualOrientation)
            {
                case Orientation.HorizontalPos:
                    actualOrientation = Orientation.VerticalPos;
                    break;
                case Orientation.HorizontalNeg:
                    actualOrientation = Orientation.VerticalNeg;
                    break;
                case Orientation.VerticalPos:
                    actualOrientation = Orientation.HorizontalNeg;
                    break;
                case Orientation.VerticalNeg:
                    actualOrientation = Orientation.HorizontalPos; ;
                    break;
            }
        }

        public void orientationToLeft()
        {
            switch (actualOrientation)
            {
                case Orientation.HorizontalPos:
                    actualOrientation = Orientation.VerticalNeg;
                    break;
                case Orientation.HorizontalNeg:
                    actualOrientation = Orientation.VerticalPos;
                    break;
                case Orientation.VerticalPos:
                    actualOrientation = Orientation.HorizontalPos;
                    break;
                case Orientation.VerticalNeg:
                    actualOrientation = Orientation.HorizontalNeg; ;
                    break;
            }
        }

        public Orientation getActualOrientation()
        {
            return actualOrientation;
        }

    }
}
