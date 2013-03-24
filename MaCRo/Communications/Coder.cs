using System;
using Microsoft.SPOT;
using MaCRo.Tools;
using System.Text;
using MaCRo.Core;
using System.Globalization;

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

                    case Message.MagX:
                        buffer = new byte[2+2];
                        buffer[0] = (byte)'m';
                        buffer[1] = (byte)'x';
                        break;

                     case Message.MagY:
                        buffer = new byte[2+2];
                        buffer[0] = (byte)'m';
                        buffer[1] = (byte)'y';
                        break;
                    case Message.MagZ:
                        buffer = new byte[4];
                        buffer[0] = (byte)'m';
                        buffer[1] = (byte)'z';
                        break;
                    case Message.Info:
                        buffer = new byte[4 + (value as string).Length];
                        buffer[0] = (byte)'L';
                        buffer[1] = (byte)'I';
                        break;
                    case Message.Debug:
                        buffer = new byte[4 + (value as string).Length];
                        buffer[0] = (byte)'L';
                        buffer[1] = (byte)'D';
                        break;
                    case Message.Error:
                        buffer = new byte[4 + (value as string).Length];
                        buffer[0] = (byte)'L';
                        buffer[1] = (byte)'E';
                        break;
                    
                    case Message.Voltage:
                        buffer = new byte[((double)value).ToString().Length + 2 + 2];
                        buffer[0] = (byte)'b';
                        buffer[1] = (byte)'v';
                        break;
                    case Message.Current:
                        buffer = new byte[((double)value).ToString().Length + 2 + 2];
                        buffer[0] = (byte)'b';
                        buffer[1] = (byte)'i';
                        break;
                    case Message.Capacity:
                        buffer = new byte[2 + 2];
                        buffer[0] = (byte)'b';
                        buffer[1] = (byte)'p';
                        break;
                    case Message.Estimation:
                        buffer = new byte[2 + 2];
                        buffer[0] = (byte)'b';
                        buffer[1] = (byte)'e';
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
                else if (value is string)
                {
                    this.FromString(value as string, buffer, 2);
                }
                else if (value is ushort)
                {
                    this.FromUShort((ushort)value, buffer, 2);
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
                    Engine.getInstance().ManualBackward(ToShort(tmpBuff, 1));
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

                case 'V':
                    Engine.getInstance().ManualSpeed(ToShort(tmpBuff, 1));
                    break;
                case 'v':
                    Engine.getInstance().ManualTurningSpeed(ToShort(tmpBuff, 1));
                    break;

                case 'p':
                    int readen = 1;
                    int temp;
                    Position p = new Position();
                    double x = ToDouble(tmpBuff, readen, out temp);
                    readen += temp;
                    //Engine.getInstance().Debug("X: " + x.ToString());

                    double y = ToDouble(tmpBuff, readen, out temp);
                    readen += temp;

                    double angle = ToDouble(tmpBuff, readen, out temp);
                    readen += temp;

                    p.x = x;
                    p.y = y;
                    p.angle = angle;

                    Engine.getInstance().UpdatePosition(p);
                    break;
                case 'c':
                    short value = 0;
                    switch ((char)tmpBuff[1])
                    {
                        case 'x':
                            value = ToShort(tmpBuff, 2);
                            Engine.getInstance().calibrarX(value);
                            break;
                        case 'y':
                            value = ToShort(tmpBuff, 2);
                            Engine.getInstance().calibrarY(value);
                            break;
                    }
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

        private int FromDouble(double theDouble, byte[] buffer, int offset)
        {
            int Write = FromString(theDouble.ToString(), buffer, offset);

            return Write;
        }

        private double ToDouble(byte[] buffer, int offset, out int read)
        {
            string s;
            read = ToString(buffer, offset, out s);
            //Engine.getInstance().Debug("String: " + s);

            double d = double.Parse(s);

            if (s[0] == '-' && d > 0)
                d *= -1;

            return d;
        }

        private int FromString(string theString, byte[] buffer, int offset)
        {
            byte[] tmpBuffer = Encoding.UTF8.GetBytes(theString);

            // Prefix string with int length
            FromShort((short)tmpBuffer.Length, buffer, offset);
            int written = sizeof(short);

            // Copy to output buffer
            Array.Copy(tmpBuffer, 0, buffer, offset + written, tmpBuffer.Length);
            written += tmpBuffer.Length;

            return written;
        }

        private int ToString(byte[] buffer, int offset, out string theString)
        {
            // Strings are prefixed with an integer size
            short len = ToShort(buffer, offset);

            // Encoding's GetChars() converts an entire buffer, so extract just the string
            //part
            byte[] tmpBuffer = new byte[len];
            int dst = offset + sizeof(int);

            Array.Copy(buffer, dst, tmpBuffer, 0, len);

            theString = new string(Encoding.UTF8.GetChars(tmpBuffer));

            return (dst + len) - offset;
        }
    }
}
