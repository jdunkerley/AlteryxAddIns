using System.Xml;

namespace OmniBus.Framework.Serialisation
{
    /// <summary>
    /// Interface for Serialisers
    /// </summary>
    /// <typeparam name="TConfig">Type of object to serialialise</typeparam>
    public interface ISerialiser<TConfig> where TConfig : new()
    {
        /// <summary>
        /// Serialise an object to an Xml Element
        /// </summary>
        /// <param name="source">Object to serialise</param>
        /// <returns>Serialied Node</returns>
        XmlNode Serialise(TConfig source);

        /// <summary>
        /// Deserialise an object from an Xml Element
        /// </summary>
        /// <param name="node">Serialied Node</param>
        /// <returns>Deserialised Object</returns>
        TConfig Deserialise(XmlNode node);
    }
}