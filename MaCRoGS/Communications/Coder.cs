using System;
using System.Text;
using System.Globalization;

namespace MaCRoGS.Communications
{
    public class Coder
    {
        SerialTransport transport;
        SerialTransportAddress t;
        MainWindow display;

        public void Start(MainWindow display)
        {
            this.display = display;
            t = new SerialTransportAddress("COM3", false);
            transport = new SerialTransport(t, 9600);
            transport.Start(this);
        }
        public void Send(Message message, object value)
        {
            //1 byte: movement/telemtry
            //1 byte: right/left - forward/backward
            //2 bytes: (short) distance in centimeters or angle in degrees
            byte[] buffer;
            switch (message)
            {
                case Message.Forward:
                    buffer = new byte[3];
                    buffer[0] = (byte)'f';
                    this.FromShort((short)value, buffer, 1);
                    break;
                case Message.Backward:
                    buffer = new byte[3];
                    buffer[0] = (byte)'b';
                    this.FromShort((short)value, buffer, 1);
                    break;
                case Message.TurnLeft:
                    buffer = new byte[1];
                    buffer[0] = (byte)'l';
                    break;
                case Message.TurnRight:
                    buffer = new byte[1];
                    buffer[0] = (byte)'r';
                    break;
                case Message.Stop:
                    buffer = new byte[1];
                    buffer[0] = (byte)'s';
                    break;
                case Message.ToManual:
                    buffer = new byte[1];
                    buffer[0] = (byte)'m';
                    break;
                case Message.StopManual:
                    buffer = new byte[1];
                    buffer[0] = (byte)'M';
                    break;
                case Message.Speed:
                    buffer = new byte[3];
                    buffer[0] = (byte)'V';
                    this.FromShort((short)value, buffer, 1);
                    break;
                case Message.TurningSpeed:
                    buffer = new byte[3];
                    buffer[0] = (byte)'v';
                    this.FromShort((short)value, buffer, 1);
                    break;
                case Message.UpdatePosition:
                    int offset = 1;
                    Position p = value as Position;
                    buffer = new byte[p.x.ToString().Length + p.y.ToString().Length + p.angle.ToString().Length + 12 + 1];
                    buffer[0] = (byte)'p';
                    offset += FromDouble(p.x, buffer, offset);
                    offset += FromDouble(p.y, buffer, offset);
                    offset += FromDouble(p.angle, buffer, offset);
                    break;
                case Message.ValorCalibrarX:
                    buffer = new byte[4];
                    buffer[0] = (byte)'c';
                    buffer[1] = (byte)'x';
                    this.FromShort((short)value, buffer, 2);
                    break;
                case Message.ValorCalibrarY:
                    buffer = new byte[4];
                    buffer[0] = (byte)'c';
                    buffer[1] = (byte)'y';
                    this.FromShort((short)value, buffer, 2);
                    break;
                /*case Message.SendPID:
                    buffer = new byte[2];
                    buffer[0] = (byte)'P';
                    this.FromShort((short)value, buffer, 1);
                    break;*/
                default:
                    buffer = new byte[0];
                    break;
            }

            transport.Send(t, buffer);
        }

        public void DataReceived(byte[] buffer, int offset, int length, TransportAddress t)
        {
            byte[] tmpBuff = new byte[length];
            for (int i = 0; i < length; i++)
            {
                tmpBuff[i] = buffer[offset + i];
            }

            object value;
            switch ((char)tmpBuff[0])
            {
                case 't':
                    switch ((char)tmpBuff[1])
                    {
                        case 'S':
                            value = ToShort(tmpBuff, 2);
                            display.UpdateS1((short)value);
                            break;
                        case 's':
                            value = ToShort(tmpBuff, 2);
                            display.UpdateS2((short)value);
                            break;
                        case 'L':
                            value = ToShort(tmpBuff, 2);
                            display.UpdateL1((short)value);
                            break;
                        case 'l':
                            value = ToShort(tmpBuff, 2);
                            display.UpdateL2((short)value);
                            break;
                        default:
                            break;
                    }
                    break;
                case 'p':
                    switch ((char)tmpBuff[1])
                    {
                        case 'x':
                            value = ToDouble(tmpBuff, 2);
                            display.SetPositionX((double)value);
                            break;
                        case 'y':
                            value = ToDouble(tmpBuff, 2);
                            display.SetPositionY((double)value);
                            break;
                        case 'a':
                            value = ToDouble(tmpBuff, 2);
                            display.SetPositionAngle((double)value);
                            break;
                    }
                    break;
                case 'm':
                    switch ((char)tmpBuff[1])
                    {
                        case 'x':
                            value = ToShort(tmpBuff, 2);
                            display.SetMagX((short)value);
                            break;
                        case 'y':
                            value = ToShort(tmpBuff, 2);
                            display.SetMagY((short)value);
                            break;
                        case 'z':
                            value = ToShort(tmpBuff, 2);
                            display.SetMagZ((short)value);
                            break;
                    }
                    break;
                case 'L':
                    string message;
                    switch ((char)tmpBuff[1])
                    {
                        case 'D':
                            //DEBUG
                            ToString(tmpBuff, 2, out message);
                            display.debug(message);
                            break;
                        case 'I':
                            //INFO         
                            ToString(tmpBuff, 2, out message);
                            display.info(message);
                            break;
                        case 'E':
                            //ERROR
                            ToString(tmpBuff, 2, out message);
                            display.error(message);
                            break;
                    }
                    break;

                case 'b':
                    switch ((char)tmpBuff[1])
                    {
                        case 'v':
                            //voltage
                            double voltage = ToDouble(tmpBuff, 2);
                            display.setVoltage(voltage);
                            break;
                        case 'i':
                            //current         
                            double current = ToDouble(tmpBuff, 2);
                            display.setCurrent(current);
                            break;
                        case 'p':
                            //percent
                            ushort Rvoltage = ToUShort(tmpBuff, 2);
                            display.setRVoltage(Rvoltage);
                            break;
                        case 'e':
                            //estimation
                            ushort estimation = ToUShort(tmpBuff, 2);
                            display.setEstimation(estimation);
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        private void FromShort(short theUShort, byte[] buffer, int offset)
        {
            unchecked
            {
                buffer[offset] = (byte)theUShort;
                buffer[offset + 1] = (byte)(theUShort >> 8);
            }
        }

        private short ToShort(byte[] buffer, int offset)
        {
            return (short)(
               (buffer[offset] & 0x000000FF) |
               (buffer[offset + 1] << 8 & 0x0000FF00)
               );
        }

        private ushort ToUShort(byte[] buffer, int offset)
        {
            return (ushort)(
               (buffer[offset] & 0x000000FF) |
               (buffer[offset + 1] << 8 & 0x0000FF00)
               );
        }

        private int ToString(byte[] buffer, int offset, out string theString)
        {
            // Strings are prefixed with an integer size
            short len = ToShort(buffer, offset);

            // Encoding's GetChars() converts an entire buffer, so extract just the string
            //part
            byte[] tmpBuffer = new byte[len];
            int dst = offset + sizeof(short);

            Array.Copy(buffer, dst, tmpBuffer, 0, len);

            theString = new string(Encoding.UTF8.GetChars(tmpBuffer));

            return (dst + len) - offset;
        }

        private double ToDouble(byte[] buffer, int offset)
        {
            string s;
            ToString(buffer, offset, out s);

            double d = double.Parse(s, CultureInfo.InvariantCulture);

            return d;
        }

        public int FromDouble(double theDouble, byte[] buffer, int offset)
        {
            int Write = FromString(theDouble.ToString().Replace(',', '.'), buffer, offset);

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
