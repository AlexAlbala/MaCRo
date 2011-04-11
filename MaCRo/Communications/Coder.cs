using System;
using Microsoft.SPOT;
using MaCRo.Tools;

namespace MaCRo.Communications
{
    class Coder
    {        
        SerialTransport transport;
        SerialTransportAddress t;

        public void Start()
        {
            t = new SerialTransportAddress("COM1", false);
            transport = new SerialTransport(t, 9600);
            transport.Start(this);
        }
        public void Send(Message message, ushort value)
        {
            //1 byte: movement/telemtry
            //1 byte: right/left - forward/backward
            //2 bytes: (short) distance in centimeters or angle in degrees
            byte[] buffer = new byte[4];
            switch (message)
            {
                case Message.MoveBackward:
                    buffer[0] = (byte)'m';
                    buffer[1] = (byte)'b';
                    break;
                case Message.MoveForward:
                    buffer[0] = (byte)'m';
                    buffer[1] = (byte)'f';
                    break;
                case Message.TurnLeft:
                    buffer[0] = (byte)'m';
                    buffer[1] = (byte)'l';
                    break;
                case Message.TurnRight:
                    buffer[0] = (byte)'m';
                    buffer[1] = (byte)'r';
                    break;
                case Message.Stop:
                    buffer[0] = (byte)'m';
                    buffer[1] = (byte)'s';
                    break;
                case Message.Object:
                    buffer[0] = (byte)'t';
                    buffer[1] = (byte)'o';
                    break;
                case Message.SensorS1:
                    buffer[0] = (byte)'t';
                    buffer[1] = (byte)'S';
                    break;
                case Message.SensorS2:
                    buffer[0] = (byte)'t';
                    buffer[1] = (byte)'s';
                    break;
                case Message.SensorL1:
                    buffer[0] = (byte)'t';
                    buffer[1] = (byte)'L';
                    break;
                case Message.SensorL2:
                    buffer[0] = (byte)'t';
                    buffer[1] = (byte)'l';
                    break;
                case Message.Mode:
                    buffer[0] = (byte)'m';
                    buffer[1] = (byte)'m';
                    break;
                default:
                    break;
            }

            this.FromUShort(value, buffer, 2);
            transport.Send(t, buffer);
        }

        public void DataReceived(byte[] buffer, int offset, int length, TransportAddress t)
        {
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
