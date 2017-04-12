using System.Xml;

namespace OmniBus.Framework.Serialisation
{
    /// <summary>
    /// Interface for serialisers
    /// </summary>
    internal interface ITypeSerialiser
    {
        /// <summary>
        /// Given an object, serialise to an XmlElement
        /// </summary>
        /// <param name="doc">Parent Xml Document</param>
        /// <param name="name">Name for Node</param>
        /// <param name="value">Object to serialise</param>
        /// <returns>Serialised node</returns>
        XmlElement Serialise(XmlDocument doc, string name, object value);

        /// <summary>
        /// Given an XmlElement, deserialise to an object
        /// </summary>
        /// <param name="node">Serialised node</param>
        /// <returns>Object</returns>
        object Deserialise(XmlElement node);
    }
}