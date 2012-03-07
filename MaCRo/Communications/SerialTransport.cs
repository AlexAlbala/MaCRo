using System;
using System.Reflection;
using System.Text;
using System.Threading;
using System.IO.Ports;

using MaCRo.Tools;

namespace MaCRo.Communications
{
    internal class SerialTransport
    {
        //protected static readonly ILog log = LogManager.GetLogger("Marea.Transport.Serial");
        public enum MagicByte : byte
        {
            FAST = 171,
            SAFE = 018
        };

        protected Dictionary SendBuffer;
        protected bool end;

        protected Thread receive;
        protected Thread reSend;
        protected SerialPort port;
        protected SerialTransportAddress sta;
        protected SafeSerialHeader headerACK;
        protected Coder coder;
        protected byte sequence;


        public SerialTransport(SerialTransportAddress sta, int baudRate)
        {
            this.sta = sta;
            this.port = CreateSerialPort(sta, baudRate);

            SendBuffer = new Dictionary();
            headerACK = new SafeSerialHeader();
        }

        /// <summary>
        /// Override this method in order to perform a custom Serial Port initialization.
        /// </summary>
        /// <param name="baudRate">Serial port baud rate.</param>
        /// <returns>Ready-to-use serial port.</returns>
        internal virtual SerialPort CreateSerialPort(SerialTransportAddress sta, int baudRate)
        {
            SerialPort port;
            port = new SerialPort(sta.serialport, baudRate, Parity.None, 8, StopBits.One);
#if !MicroFramework
            port.ReadBufferSize = 65536;
            port.Encoding = Encoding.GetEncoding("iso-8859-1");
#endif
            return port;
        }

        public virtual void Start(Coder coder)
        {
            this.coder = coder;
            ThreadStart ts = new ThreadStart(this.Receive);
            receive = new Thread(ts);

            ThreadStart ts2 = new ThreadStart(this.ReSend);
            reSend = new Thread(ts2);

            end = false;
            port.Open();
            receive.Start();
            reSend.Start();
        }

        public virtual void Stop()
        {
            end = true;
            lock (port)
            {
                port.Close();
            }
        }

        private byte GetSequence()
        {
            lock (SendBuffer)
            {
                do
                {
                    if (sequence == byte.MaxValue) sequence = 0;
                    else sequence++;
                } while (SendBuffer.ContainsKey(sequence));
            }
            return sequence;
        }

        public virtual void Send(TransportAddress t, byte[] data)
        {
            byte[] tmp;
            SerialHeader header;

            if (data.Length > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("there are too many bytes to transmit");

            // Check if this message is going to use Acknowledgements
            if (((SerialTransportAddress)t).forceACK) { header = new SafeSerialHeader(); }
            else { header = new SerialHeader(); }

            header.length = (ushort)data.Length;
            if (header is SafeSerialHeader)
            {
                ((SafeSerialHeader)header).sequence = GetSequence();
            }
            tmp = new byte[header.size + data.Length];
            header.GetBytes().CopyTo(tmp, 0);
            data.CopyTo(tmp, header.size);

            header.CRC = SerialCRC.Calculate(tmp, (uint)tmp.Length);
            header.GetBytes().CopyTo(tmp, 0);

            if (header is SafeSerialHeader)
            {
                lock (SendBuffer)
                {
                    SendBuffer.Add(sequence, new SafeData(tmp));
                }
            }
            try
            {
                WriteToPort(tmp, 0, tmp.Length, header is SafeSerialHeader);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        internal void ReSend()
        {
            byte[] data = null;

            while (!end)
            {
                try
                {
                    Thread.Sleep(100);
                    lock (SendBuffer)
                    {
                        foreach (SafeData sd in SendBuffer.Values)
                        {
                            if (sd.count < 100)
                            {
                                data = sd.data;
                                WriteToPort(data, 0, data.Length, true);

                                // TODO: improve delay calculation!
                                sd.delay += (uint)(sd.delay / 2);

                                sd.count = sd.delay;
                            }
                            else
                            {
                                sd.count -= 100;
                            }
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    //It's safe to ignore this :)
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        protected virtual void WriteToPort(byte[] buffer, int offset, int count, bool safe)
        {
            lock (port)
            {
                port.Write(buffer, offset, count);
            }
        }

        public virtual void Receive()
        {
            int n;
            byte[] buffer = new byte[1000]; // Max UDP Datagram ?????¿??
            SerialHeader sh;

            while (!end)
            {
                try
                {
                    // Receive magic byte
                    n = 0;
                    do { n += port.Read(buffer, 0, 1); } while (n < 1);
                    if (buffer[0] == (byte)MagicByte.SAFE)
                    {
                        sh = new SafeSerialHeader();
                    }
                    else if (buffer[0] == (byte)MagicByte.FAST)
                    {
                        sh = new SerialHeader();
                    }
                    else
                    {
                        continue;
                    }
                    // Receive the header
                    do { n += port.Read(buffer, n, sh.size - n); } while (n < sh.size);
                    if (!sh.Populate(buffer, 1))
                    {
                        continue;
                    }
                    // Receive and process the data
                    n = 0;
                    do { n += port.Read(buffer, sh.size + n, sh.length - n); } while (n < sh.length);

                    if (sh.CRC == SerialCRC.Calculate(buffer, (uint)(sh.length + sh.size)))
                    {
                        ProcessPacket(sh, buffer, sh.size);
                    }
                    else
                    {
                    }
                }
                catch (ThreadAbortException)
                {
                    //It's safe to ignore this :)
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        internal void ProcessPacket(SerialHeader sh, byte[] buffer, int offset)
        {
            if (sh is SafeSerialHeader)
            {
                if (sh.length > 0)
                {
                    headerACK.CRC = 0;
                    headerACK.sequence = ((SafeSerialHeader)sh).sequence;
                    headerACK.CRC = SerialCRC.Calculate(headerACK.GetBytes(), headerACK.size);
                    WriteToPort(headerACK.GetBytes(), 0, headerACK.size, true);
                }
                else
                {
                    byte seq = ((SafeSerialHeader)sh).sequence;
                    lock (SendBuffer)
                    {
                        if (SendBuffer.ContainsKey(seq))
                        {
                            SendBuffer.Remove(seq);
                        }
                    }
                }
            }
            if (sh.length > 0)
                coder.DataReceived(buffer, offset, sh.length, sta);
        }

        public TransportAddress GetTransportAddress()
        {
            return sta;
        }
    }

    public class SerialHeader
    {
        public byte magic;
        public ushort length;
        public uint CRC;
        public ushort size;

        public SerialHeader()
        {
            magic = (byte)SerialTransport.MagicByte.FAST;
            length = 0;
            CRC = 0;
            size = 7;
        }

        public virtual byte[] GetBytes()
        {
            byte[] tmp = new byte[size];

            tmp[0] = magic;
            BitConverter.GetBytes(length).CopyTo(tmp, 1);
            BitConverter.GetBytes(CRC).CopyTo(tmp, 3);

            return tmp;
        }

        /// <summary>
        /// Retrieves header data and sets buffer CRC positions to zero
        /// </summary>
        public virtual bool Populate(byte[] buffer, int offset)
        {
            length = BitConverter.ToUInt16(buffer, offset);
            if (length > ushort.MaxValue - this.size)
                return false;
            CRC = BitConverter.ToUInt32(buffer, 2 + offset);
            buffer[2 + offset] = 0;
            buffer[3 + offset] = 0;
            buffer[4 + offset] = 0;
            buffer[5 + offset] = 0;
            return true;
        }
    }

    public class SafeSerialHeader : SerialHeader
    {
        public byte sequence;
        public static ushort Size = 8;

        public SafeSerialHeader()
        {
            magic = (byte)SerialTransport.MagicByte.SAFE;
            length = 0;
            CRC = 0;
            size = Size;
        }

        public override byte[] GetBytes()
        {
            byte[] tmp = new byte[size];

            base.GetBytes().CopyTo(tmp, 0);
            tmp[size - 1] = sequence;

            return tmp;
        }

        public override bool Populate(byte[] buffer, int offset)
        {
            bool tmp = base.Populate(buffer, offset);
            sequence = buffer[size - 2 + offset];
            return tmp;
        }
    }

    public class SafeData
    {
        public byte[] data;
        public uint delay;
        public uint count;

        public SafeData(byte[] data)
        {
            this.data = data;
            delay = 2000;
            count = delay;
        }
    }
}