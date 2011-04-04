using System;

namespace MaCRo.Communications
{
    /// <summary>
    /// Enumerator of all the available Transport Modes
    /// </summary>
    public enum TransportMode
    {
        /// <summary>
        /// UDP Transport
        /// </summary>
        UDP,
        /// <summary>
        /// TCP Transport
        /// </summary>        
        TCP,
        /// <summary>
        /// Persistent TCP Transport
        /// </summary>        
        PersistentTCP,
        /// <summary>
        /// Serial Port Transport
        /// </summary>
        Serial
    };

    /// <summary>
    /// Defines a Marea generic Transport Address
    /// </summary>
    [Serializable]
    public abstract class TransportAddress
    {
        public TransportAddress() { }
        /// <summary>
        /// The transport mode of this Transport Address
        /// </summary>
        public TransportMode transportMode;

        /// <summary>
        /// Create a deep copy of the Transport Address
        /// </summary>
        abstract public TransportAddress Clone();

        /// <summary>
        /// Create a deep copy of the Transport Address
        /// </summary>
        abstract public string GetAddress();

        /// <summary>
        /// Check if a Tranport Address belongs to the same network
        /// </summary>
        abstract public bool IsSameNetwork(TransportAddress ta);
    }
}
