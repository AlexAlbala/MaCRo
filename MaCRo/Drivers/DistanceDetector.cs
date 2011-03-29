/*
Copyright 2010 GHI Electronics LLC
Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
http://www.apache.org/licenses/LICENSE-2.0
Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License. 
*/

using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.Hardware;
using GHIElectronics.NETMF.FEZ;

namespace MaCRo.Drivers
{
    public class DistanceDetector : IDisposable
    {
        AnalogIn adc;
        float Y0 = 0;
        float X0 = 0;
        float Y1 = 0;
        float X1 = 0;
        float C = 0;

        public enum SharpSensorType : byte
        {
            GP2Y0A21YK = 0,
            GP2D120 = 1,
        }

        public void Dispose()
        {
            adc.Dispose();
        }

        public DistanceDetector(FEZ_Pin.AnalogIn pin, SharpSensorType type)
        {
            adc = new AnalogIn((AnalogIn.Pin)pin);
            adc.SetLinearScale(0, 330);
            switch (type)
            {
                case SharpSensorType.GP2Y0A21YK:
                    Y0 = 10;
                    X0 = 315;
                    Y1 = 80;
                    X1 = 30;
                    break;

                case SharpSensorType.GP2D120:
                    Y0 = 3;
                    X0 = 315;
                    Y1 = 30;
                    X1 = 30;
                    break;
            }
            C = (Y1 - Y0) / (1 / X1 - 1 / X0);
        }

        public float GetDistance_cm()
        {
            return C / ((float)adc.Read() + (float).001) - (C / X0) + Y0;
        }
    }
}
