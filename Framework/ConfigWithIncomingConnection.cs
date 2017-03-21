using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

using OmniBus.Framework.Interfaces;

namespace OmniBus.Framework
{
    /// <summary>
    ///     Base Class For Configuration Objects Which Need The Incoming Connection.
    /// </summary>
    public abstract class ConfigWithIncomingConnection : IConfigWithIncomingConnection
    {
        /// <summary>
        ///     Gets or sets the meta data for the incoming connections.
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public XmlElement[] IncomingMetaInfo { get; set; }
    }
}