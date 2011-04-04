using System;

namespace MaCRo.Communications
{
    /// <summary>
    /// Transport Address for the Serial protocol
    /// </summary>
    [Serializable]
    public class SerialTransportAddress : TransportAddress
    {
        public SerialTransportAddress() { }

        /// <summary>
        /// The remote serial port
        /// </summary>
        public string serialport;

        /// <summary>
        /// Sets if ACK should be used
        /// </summary>
        public bool forceACK;

        /// <summary>
        /// Creates new TransportAddress instance
        /// </summary>
        /// <param name="port">The remote Port</param>
        /// <param name="forceACK">Forces ACK usage</param>
        public SerialTransportAddress(string port, bool forceACK)
        {
            transportMode = TransportMode.Serial;
            serialport = port;
            this.forceACK = forceACK;
        }

        /// <summary>
        /// Indicates if this object equals to an other object
        /// </summary>
        /// <param name="obj">Object used to compare</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) return false;
            SerialTransportAddress tm = (SerialTransportAddress)obj;
            return transportMode.Equals(tm.transportMode) && serialport.Equals(tm.serialport);
        }

        /// <summary>
        /// Gets a no unique identifier 
        /// </summary>
        public override int GetHashCode()
        {
            return serialport.GetHashCode() + transportMode.GetHashCode();
        }

        /// <summary>
        /// Get the transport address in string format.
        /// </summary>
        /// <returns>Returns the ip address, port and Transport Layer type.</returns>
        public override string ToString()
        {
            return serialport + "/" + transportMode.ToString();
        }

        /// <summary>
        /// Get string for "Matricula" identifier system.
        /// </summary>
        /// <returns>Matricula of Marea.</returns>
        public override string GetAddress()
        {
            return serialport;
        }

        /// <summary>
        /// Check if a Tranport Address belongs to the same network as this one.
        /// </summary>
        public override bool IsSameNetwork(TransportAddress ta)
        {
            if (ta == null || GetType() != ta.GetType()) return false;
            SerialTransportAddress t = (SerialTransportAddress)ta;
            return (serialport == t.serialport);
        }

        /// <summary>
        /// Create a deep copy of the Serial Transport Address
        /// </summary>
        public override TransportAddress Clone()
        {
            return new SerialTransportAddress((String)serialport, forceACK);
        }
    }
}
