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
        public void Send(Message message, short value)
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

                case Message.IMUAccX:
                    buffer[0] = (byte)'A';
                    buffer[1] = (byte)'X';
                    break;
                case Message.IMUGyrX:
                    buffer[0] = (byte)'G';
                    buffer[1] = (byte)'X';
                    break;
                case Message.IMUMagX:
                    buffer[0] = (byte)'M';
                    buffer[1] = (byte)'X';
                    break;
                case Message.IMUTempX:
                    buffer[0] = (byte)'T';
                    buffer[1] = (byte)'X';
                    break;

                case Message.IMUAccY:
                    buffer[0] = (byte)'A';
                    buffer[1] = (byte)'Y';
                    break;
                case Message.IMUGyrY:
                    buffer[0] = (byte)'G';
                    buffer[1] = (byte)'Y';
                    break;
                case Message.IMUMagY:
                    buffer[0] = (byte)'M';
                    buffer[1] = (byte)'Y';
                    break;
                case Message.IMUTempY:
                    buffer[0] = (byte)'T';
                    buffer[1] = (byte)'Y';
                    break;

                case Message.IMUAccZ:
                    buffer[0] = (byte)'A';
                    buffer[1] = (byte)'Z';
                    break;
                case Message.IMUGyrZ:
                    buffer[0] = (byte)'G';
                    buffer[1] = (byte)'Z';
                    break;
                case Message.IMUMagZ:
                    buffer[0] = (byte)'M';
                    buffer[1] = (byte)'Z';
                    break;
                case Message.IMUTempZ:
                    buffer[0] = (byte)'T';
                    buffer[1] = (byte)'Z';
                    break;
                default:
                    break;
            }

            this.FromShort(value, buffer, 2);
            transport.Send(t, buffer);
        }

        public void DataReceived(byte[] buffer, int offset, int length, TransportAddress t)
        {
        }

        private void FromShort(short theShort, byte[] buffer, int offset)
        {
            unchecked
            {
                buffer[offset] = (byte)theShort;
                buffer[offset + 1] = (byte)(theShort >> 8);
            }
        }

        private short ToShort(byte[] buffer, int offset)
        {
            return (short)(
               (buffer[offset] & 0x000000FF) |
               (buffer[offset + 1] << 8 & 0x0000FF00)
               );
        }
    }
}
