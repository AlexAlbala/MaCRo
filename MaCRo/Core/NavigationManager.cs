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
        private Timer act_posTimer;

        public bool Pid;
        int K;

        public sbyte manualSpeed { set; get; }
        public sbyte manualTurningSpeed { set; get; }

        public double distance_mm { get { return (left.distance_mm + right.distance_mm) / 2; } }

        public double MAG_Heading { get { return magnetometer.MAGHeadingRad; } }

        public double Relative_MAG_Heading { get { return MAG_Heading - initialHeading; } }

        #region IMU
  

        public short getMag(Axis axis)
        {
            short[] mag = magnetometer.getMag();
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
        public void SetXOFF(short _cx)
        {
            Engine.getInstance().Info("Ola que ase" + _cx);
            magnetometer.SetXOFF(_cx);
        }

        public void SetYOFF(short _cy)
        {
            Engine.getInstance().Info("ola que ase2" + _cy);
            magnetometer.SetYOFF(_cy);
        }
       

        #endregion

        public NavigationManager()
        {
            movement = Movement.stop;
            monitor = new object();
            actualPosition = new Position();

            //TODO
            //Waiting for the magnetometer...
            Thread.Sleep(1500);
            initialHeading = this.MAG_Heading;

            contingency = new Contingency(left, right, this);
            //integration = new Timer(new TimerCallback(this.Integrate), new object(), GlobalVal.integrationPeriod * 10, GlobalVal.integrationPeriod);
        
        }

        public Position getActualPosition()
        {
            actualPosition.angle = this.Relative_MAG_Heading;
            Debug.Print("angle_gir:" + this.actualPosition.angle.ToString());
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
            Pid = false;
            float central, wall, wall_back;
            //double distancia_pared;
            K = 10;

            if (Pid == true)
            {
                central = sensors.getDistance(Sensor.Central);
                turnRight((int)(central / (central - GlobalVal.minDistanceToFollowWall)));

                while (!contingency.alarm)
                {
                    wall = sensors.getDistance(Sensor.Wall);
                    wall_back = sensors.getDistance(Sensor.wall_back);
                    if (exMath.Abs(wall - wall_back) <= GlobalVal.hysteresis)
                        break;
                    else
                    {
                        central = sensors.getDistance(Sensor.Central);
                        //distancia_pared = (exMath.cos(Relative_MAG_Heading)) * (central);
                        //turnRight((int)(distancia_pared / (distancia_pared - GlobalVal.minDistanceToFollowWall)) * K);
                    }
                    MoveForward(5);
                }

            }

            if (Pid == false)
            {
                brake();
                int s=55;
                turnRight(s);

                Debug.Print("angle_Def:" + s.ToString());
                //Thread.Sleep(600);
                while (!contingency.alarm)
                {
                    wall = sensors.getDistance(Sensor.Wall);
                    wall_back = sensors.getDistance(Sensor.wall_back);

                    if (exMath.Abs(wall - wall_back) <= GlobalVal.hysteresis)
                        break;
                    else
                        turnRight(5);
                }
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
                        turnLeft(10);
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
            Engine.getInstance().Info("initial:" + initialHeading);
            Engine.getInstance().Info("anglepos:" + angle);


            Debug.Print("initialangle:" + initialHeading.ToString());
            //Let's suppose the mass center is in the geometrical center of the rover
            double lengthRight = angleRad * GlobalVal.width_correction * GlobalVal.distanceBetweenWheels_mm / 2;
            double lengthLeft = lengthRight;

            turnRight();

            while (right.distance_mm < lengthRight && left.distance_mm < lengthLeft && !contingency.alarm)
            {
                Thread.Sleep(50);
            }
            this.brake();

            //actualPosition.angle += (left.distance_mm + right.distance_mm) / (GlobalVal.width_correction * GlobalVal.distanceBetweenWheels_mm);
           
            actualPosition.angle = this.Relative_MAG_Heading;
            Debug.Print("angle:"+ this.MAG_Heading.ToString());
            Debug.Print("angle_gir:" + this.actualPosition.angle.ToString());
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

            //actualPosition.angle += (left.distance_mm + right.distance_mm) / (GlobalVal.width_correction * GlobalVal.distanceBetweenWheels_mm);
            actualPosition.angle = this.Relative_MAG_Heading;
        }

        private void turnRight()
        {
            Debug.Print("hola");
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

            //actualPosition.angle -= (left.distance_mm + right.distance_mm) / (GlobalVal.width_correction * GlobalVal.distanceBetweenWheels_mm);
            
            actualPosition.angle = this.Relative_MAG_Heading;
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

            //actualPosition.angle -= (left.distance_mm + right.distance_mm) / (GlobalVal.width_correction * GlobalVal.distanceBetweenWheels_mm);
            actualPosition.angle = this.Relative_MAG_Heading;
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
        /*
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
         */
        #endregion

        public void brake()
        {
           // movement = Movement.stop;
            //dcm.Move(0, 0);
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
