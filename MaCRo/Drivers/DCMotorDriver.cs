/*
Copyright 2010 GHI Electronics LLC
Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
http://www.apache.org/licenses/LICENSE-2.0
Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License. 
*/

using System;
using System.Threading;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.Hardware;
using GHIElectronics.NETMF.FEZ;
using MaCRo.Config;

namespace MaCRo.Drivers
{
    public class DCMotorDriver : IDisposable
    {
        //RIGHT MOTORS
        PWM _pwm1;
        OutputPort _dir1a;
        OutputPort _dir1b;

        //LEFT MOTORS
        PWM _pwm2;
        OutputPort _dir2a;
        OutputPort _dir2b;

        sbyte _last_speed1, _last_speed2;

        public void Dispose()
        {
            _pwm1.Dispose();
            _dir1a.Dispose();
            _dir1b.Dispose();

            _pwm2.Dispose();
            _dir2a.Dispose();
            _dir2b.Dispose();
        }

        public DCMotorDriver()
        {
            _pwm1 = new PWM((PWM.Pin)PortMap.motor1_PWM);
            _dir1a = new OutputPort((Cpu.Pin)PortMap.motor1_DirA, false);
            _dir1b = new OutputPort((Cpu.Pin)PortMap.motor1_DirB, false);

            _pwm2 = new PWM((PWM.Pin)PortMap.motor2_PWM);
            _dir2a = new OutputPort((Cpu.Pin)PortMap.motor2_DirA, false);
            _dir2b = new OutputPort((Cpu.Pin)PortMap.motor2_DirB, false);
        }

        public void Move(sbyte speed1, sbyte speed2)
        {
            if (speed1 > 100 || speed1 < -100 || speed2 > 100 || speed2 < -100)
                new ArgumentException();

            _last_speed1 = speed1;
            _last_speed2 = speed2;

            if (speed1 < 0)
            {
                _dir1a.Write(true);
                _dir1b.Write(false);
            }
            else
            {
                _dir1a.Write(false);
                _dir1b.Write(true);
            }
            _pwm1.Set(1000, (byte)Math.Abs(speed1));

            ////////////////////////////

            if (speed2 > 0) //The motor 2 is mirrored !!! Sign changed (LEFT)
            {
                _dir2a.Write(true);
                _dir2b.Write(false);
            }
            else
            {
                _dir2a.Write(false);
                _dir2b.Write(true);
            }
            _pwm2.Set(1000, (byte)Math.Abs(speed2));
        }

        public void MoveRamp(sbyte speed1, sbyte speed2, byte ramping_step_delay_mSec)
        {
            sbyte temp_speed1 = _last_speed1;
            sbyte temp_speed2 = _last_speed2;

            while ((speed1 != temp_speed1) || (speed2 != temp_speed2))
            {
                if (temp_speed1 > speed1)
                    temp_speed1--;
                if (temp_speed1 < speed1)
                    temp_speed1++;

                if (temp_speed2 > speed2)
                    temp_speed2--;
                if (temp_speed2 < speed2)
                    temp_speed2++;

                Move(temp_speed1, temp_speed2);
                Thread.Sleep(ramping_step_delay_mSec);
            }
        }
    }
}