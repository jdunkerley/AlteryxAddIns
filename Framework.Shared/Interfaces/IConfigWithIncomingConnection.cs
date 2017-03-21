using System.Xml;

namespace OmniBus.Framework.Interfaces
{
    /// <summary>
    ///     Interface for configuration objects needing incoming connection meta data
    /// </summary>
    public interface IConfigWithIncomingConnection
    {
        /// <summary>
        ///     Gets or sets the meta data for the incoming connections.
        /// </summary>
        XmlElement[] IncomingMetaInfo { get; set; }
    }
}