using System;

namespace MaCRoGS.Communications
{
    class Coder
    {        
        SerialTransport transport;
        MainWindow display;

        public void Start(MainWindow display)
        {
            this.display = display;
            transport = new SerialTransport(new SerialTransportAddress("COM3", false), 9600);
            transport.Start(this);
        }
        public void Send()
        {

        }

        public void DataReceived(byte[] buffer, int offset, int length, TransportAddress t)
        {
            byte[] tmpBuff = new byte[length];
            for (int i = 0; i < length; i++)
            {
                tmpBuff[i] = buffer[offset + i];
            }

            ushort value = ToUShort(tmpBuff, 2);
            switch ((char)tmpBuff[0])
            {             
                case 'm':
                    switch ((char)tmpBuff[1])
                    {
                        case 'f':
                            display.MoveForward(value);
                            break;
                        case 'b':
                            display.MoveBackward(value);
                            break;
                        case 'l':
                            display.TurnLeft(value);
                            break;
                        case 'r':
                            display.TurnRight(value);
                            break;
                        case 'm':
                            display.UpdateMode(value);
                            break;
                        default:
                            break;
                    }
                    break;
                case 't':
                    switch ((char)tmpBuff[1])
                    {
                        case 'S':
                            display.UpdateS1(value);
                            break;
                        case 's':
                            display.UpdateS2(value);
                            break;
                        case 'L':
                            display.UpdateL1(value);
                            break;
                        case 'l':
                            display.UpdateL2(value);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        private void FromUShort(ushort theUShort, byte[] buffer, int offset)
        {
            unchecked
            {
                buffer[offset] = (byte)theUShort;
                buffer[offset + 1] = (byte)(theUShort >> 8);
            }
        }

        private ushort ToUShort(byte[] buffer, int offset)
        {
            return (ushort)(
               (buffer[offset] & 0x000000FF) |
               (buffer[offset + 1] << 8 & 0x0000FF00)
               );
        }
    }
}
