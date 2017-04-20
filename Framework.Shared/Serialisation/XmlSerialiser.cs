using System;
using System.Xml;
using System.Xml.Serialization;

namespace OmniBus.Framework.Serialisation
{
    /// <summary>
    /// Serialiser Class using System.Xml.Serialisation
    /// </summary>
    /// <typeparam name="TConfig">Type of object to serialialise</typeparam>
    public class XmlSerialiser<TConfig> : ISerialiser<TConfig>
        where TConfig : new()
    {
        private static readonly Lazy<string> RootName = new Lazy<string>(() => new XmlSerialiser<TConfig>().Serialise(new TConfig()).Name);

        /// <summary>
        /// Serialise an object to an Xml Element
        /// </summary>
        /// <param name="source">Object to serialise</param>
        /// <returns>Serialied Node</returns>
        public XmlNode Serialise(TConfig source)
        {
            var doc = new XmlDocument();
            var serialiser = new XmlSerializer(typeof(TConfig));
            using (XmlWriter writer = doc.CreateNavigator().AppendChild())
            {
                serialiser.Serialize(writer, source);
            }

            return doc.DocumentElement;
        }

        /// <summary>
        /// Deserialise an object from an Xml Element
        /// </summary>
        /// <param name="node">Serialied Node</param>
        /// <returns>Deserialised Object</returns>
        public TConfig Deserialise(XmlNode node)
        {
            var serializer = new XmlSerializer(typeof(TConfig));
            if (node == null)
            {
                return new TConfig();
            }

            var doc = new XmlDocument();
            doc.LoadXml($"<{RootName.Value}>{node.InnerXml}</{RootName.Value}>");
            if (doc.DocumentElement == null)
            {
                return new TConfig();
            }

            return (TConfig)serializer.Deserialize(new XmlNodeReader(doc.DocumentElement));
        }
    }
}