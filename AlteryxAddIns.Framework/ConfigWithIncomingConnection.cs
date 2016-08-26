namespace JDunkerley.AlteryxAddIns.Framework
{
    using System.Xml;
    using System.Xml.Serialization;

    using Interfaces;

    /// <summary>
    /// Base Class For Configuration Objects Which Need The Incoming Connection.
    /// </summary>
    public abstract class ConfigWithIncomingConnection : IConfigWithIncomingConnection
    {
        /// <summary>
        /// Gets or sets the meta data for the incoming connections.
        /// </summary>
        [XmlIgnore]
        public XmlElement[] IncomingMetaInfo { get; set; }
    }
}
