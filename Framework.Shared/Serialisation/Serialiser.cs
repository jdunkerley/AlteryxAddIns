using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace OmniBus.Framework.Serialisation
{
    /// <summary>
    /// Serialiser Class
    /// </summary>
    /// <typeparam name="T">Type of object to serialialise</typeparam>
    public class Serialiser<T> : ISerialiser<T>
        where T : new()
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Dictionary<string, PropertyInfo> SerialisableProperties;

        static Serialiser()
        {
            SerialisableProperties =
                typeof(T).GetProperties()
                    .Where(p => p.CanWrite && p.CanRead)
                    .Where(p => p.GetCustomAttribute<XmlIgnoreAttribute>() == null)
                    .ToDictionary(p => p.Name, p => p);
        }

        /// <summary>
        /// Serialise an object to an Xml Element
        /// </summary>
        /// <param name="source">Object to serialise</param>
        /// <returns>Serialied Node</returns>
        XmlNode ISerialiser<T>.Serialise(T source)
        {
            return this.Serialise(source);
        }

        /// <summary>
        /// Serialise an object to an Xml Element
        /// </summary>
        /// <param name="source">Object to serialise</param>
        /// <param name="name">Node Name</param>
        /// <param name="doc">Reusable Xml Document</param>
        /// <returns>Serialied Node</returns>
        public XmlNode Serialise(T source, string name = "Configuration", XmlDocument doc = null)
        {
            doc = doc ?? new XmlDocument();
            var node = doc.CreateElement(name);
            if (source == null)
            {
                return node;
            }

            foreach (var serialisableProperty in SerialisableProperties.Values)
            {
                var value = serialisableProperty.GetValue(source);
                var element = Serialisers.Get(serialisableProperty.PropertyType)
                    .Serialise(doc, serialisableProperty.Name, value);
                if (element != null)
                {
                    node.AppendChild(element);
                }
            }

            return node;
        }

        /// <summary>
        /// Deserialise an object from an Xml Element
        /// </summary>
        /// <param name="node">Serialied Node</param>
        /// <returns>Deserialised Object</returns>
        public T Deserialise(XmlNode node)
        {
            var output = new T();

            if (node != null)
            {
                foreach (XmlElement nodeChildNode in node.ChildNodes)
                {
                    if (SerialisableProperties.TryGetValue(nodeChildNode.Name, out PropertyInfo serialisableProperty))
                    {
                        var value = Serialisers.Get(serialisableProperty.PropertyType).Deserialise(nodeChildNode);
                        serialisableProperty.SetValue(output, value);
                    }
                }
            }

            return output;
        }
    }
}
