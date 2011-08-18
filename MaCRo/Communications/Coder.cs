using System;
using Microsoft.SPOT;
using MaCRo.Tools;
using System.Text;
using MaCRo.Core;

namespace MaCRo.Communications
{
    class Coder
    {
        SerialTransport transport;
        SerialTransportAddress t;
        object monitor;

        public void Start()
        {
            t = new SerialTransportAddress("COM1", false);
            transport = new SerialTransport(t, 9600);
            transport.Start(this);
            monitor = new object();
        }
        public void Send(Message message, object value)
        {
            lock (monitor)
            {
                //1 byte: movement/telemtry
                //1 byte: right/left - forward/backward
                //2 bytes: (short) distance in centimeters or angle in degrees
                byte[] buffer;
                switch (message)
                {
                    case Message.SensorS1:
                        buffer = new byte[4];
                        buffer[0] = (byte)'t';
                        buffer[1] = (byte)'S';
                        break;
                    case Message.SensorS2:
                        buffer = new byte[4];
                        buffer[0] = (byte)'t';
                        buffer[1] = (byte)'s';
                        break;
                    case Message.SensorL1:
                        buffer = new byte[4];
                        buffer[0] = (byte)'t';
                        buffer[1] = (byte)'L';
                        break;
                    case Message.SensorL2:
                        buffer = new byte[4];
                        buffer[0] = (byte)'t';
                        buffer[1] = (byte)'l';
                        break;
                    case Message.Mode:
                        buffer = new byte[4];
                        buffer[0] = (byte)'m';
                        buffer[1] = (byte)'m';
                        break;


                    case Message.PositionX:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'p';
                        buffer[1] = (byte)'x';
                        break;
                    case Message.PositionY:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'p';
                        buffer[1] = (byte)'y';
                        break;
                    case Message.Angle:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'p';
                        buffer[1] = (byte)'a';
                        break;

                    case Message.Pitch:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'a';
                        buffer[1] = (byte)'p';
                        break;
                    case Message.Roll:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'a';
                        buffer[1] = (byte)'r';
                        break;
                    case Message.Yaw:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'a';
                        buffer[1] = (byte)'y';
                        break;
                    case Message.MAGHeading:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'a';
                        buffer[1] = (byte)'m';
                        break;

                    case Message.VelocityX:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'v';
                        buffer[1] = (byte)'x';
                        break;
                    case Message.VelocityY:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'v';
                        buffer[1] = (byte)'y';
                        break;

                    case Message.Time:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'v';
                        buffer[1] = (byte)'t';
                        break;

                    case Message.IMUAccX:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'A';
                        buffer[1] = (byte)'X';
                        break;
                    case Message.IMUGyrX:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'G';
                        buffer[1] = (byte)'X';
                        break;
                    case Message.IMUMagX:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'M';
                        buffer[1] = (byte)'X';
                        break;
                    case Message.IMUTempX:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'T';
                        buffer[1] = (byte)'X';
                        break;

                    case Message.IMUAccY:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'A';
                        buffer[1] = (byte)'Y';
                        break;
                    case Message.IMUGyrY:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'G';
                        buffer[1] = (byte)'Y';
                        break;
                    case Message.IMUMagY:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'M';
                        buffer[1] = (byte)'Y';
                        break;
                    case Message.IMUTempY:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'T';
                        buffer[1] = (byte)'Y';
                        break;

                    case Message.IMUAccZ:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'A';
                        buffer[1] = (byte)'Z';
                        break;
                    case Message.IMUGyrZ:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'G';
                        buffer[1] = (byte)'Z';
                        break;
                    case Message.IMUMagZ:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'M';
                        buffer[1] = (byte)'Z';
                        break;
                    case Message.IMUTempZ:
                        buffer = new byte[6 + ((double)value).ToString().Length];
                        buffer[0] = (byte)'T';
                        buffer[1] = (byte)'Z';
                        break;
                    default:
                        buffer = null;
                        break;
                }

                if (value is short)
                {
                    this.FromShort((short)value, buffer, 2);
                }
                else if (value is double)
                {
                    this.FromDouble((double)value, buffer, 2);
                }
                transport.Send(t, buffer);
            }
        }

        public void DataReceived(byte[] buffer, int offset, int length, TransportAddress t)
        {
            byte[] tmpBuff = new byte[length];
            for (int i = 0; i < length; i++)
            {
                tmpBuff[i] = buffer[offset + i];
            }
            
            switch ((char)tmpBuff[0])
            {
                case 'f':
                    Engine.getInstance().ManualForward(ToShort(tmpBuff, 1));
                    break;
                case 'b':                    
                    Engine.getInstance().ManualBackward(ToShort(tmpBuff,1));
                    break;
                case 'r':
                    Engine.getInstance().ManualRight();
                    break;
                case 'l':
                    Engine.getInstance().ManualLeft();
                    break;
                case 's':
                    Engine.getInstance().ManualStop();
                    break;
                case 'm':
                    Engine.getInstance().ManualMode();
                    break;
                case 'M':
                    Engine.getInstance().StopManualMode();
                    break;
            }
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

        public int FromDouble(double theDouble, byte[] buffer, int offset)
        {
            int Write = FromString(theDouble.ToString(), buffer, offset);

            return Write;
        }        

        public int FromString(string theString, byte[] buffer, int offset)
        {
            byte[] tmpBuffer = Encoding.UTF8.GetBytes(theString);

            // Prefix string with int length
            FromShort((short)tmpBuffer.Length, buffer, offset);
            int written = sizeof(int);

            // Copy to output buffer
            Array.Copy(tmpBuffer, 0, buffer, offset + written, tmpBuffer.Length);
            written += tmpBuffer.Length;

            return written;
        }       
    }
}
