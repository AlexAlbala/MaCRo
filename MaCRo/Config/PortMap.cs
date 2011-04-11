using System;
using Microsoft.SPOT;
using GHIElectronics.NETMF.FEZ;

namespace MaCRo.Config
{
    /// <summary>
    /// Definition of all the pins connected to board
    /// </summary>
    public static class PortMap
    {
        public readonly static FEZ_Pin.Digital debug = FEZ_Pin.Digital.LED;
        public readonly static FEZ_Pin.PWM motor1_PWM = FEZ_Pin.PWM.Di8;
        public readonly static FEZ_Pin.Digital motor1_DirA = FEZ_Pin.Digital.Di10;
        public readonly static FEZ_Pin.Digital motor1_DirB = FEZ_Pin.Digital.Di11;

        public readonly static FEZ_Pin.PWM motor2_PWM = FEZ_Pin.PWM.Di9;
        public readonly static FEZ_Pin.Digital motor2_DirA = FEZ_Pin.Digital.Di12;
        public readonly static FEZ_Pin.Digital motor2_DirB = FEZ_Pin.Digital.Di13;

        public readonly static FEZ_Pin.Interrupt encoderL_interrupt = FEZ_Pin.Interrupt.Di7;
        public readonly static FEZ_Pin.Interrupt encoderR_interrupt = FEZ_Pin.Interrupt.Di6;

        public readonly static FEZ_Pin.AnalogIn infraredS1 = FEZ_Pin.AnalogIn.An0;
        public readonly static FEZ_Pin.AnalogIn infraredS2 = FEZ_Pin.AnalogIn.An1;
        public readonly static FEZ_Pin.AnalogIn infraredL1 = FEZ_Pin.AnalogIn.An3;
        public readonly static FEZ_Pin.AnalogIn infraredL2 = FEZ_Pin.AnalogIn.An2;

    }
}
