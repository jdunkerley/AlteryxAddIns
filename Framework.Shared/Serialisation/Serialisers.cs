using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace OmniBus.Framework.Serialisation
{
    /// <summary>
    /// Static Serialiser
    /// </summary>
    internal static class Serialisers
    {
        private static readonly ConcurrentDictionary<Type, ISerialiser> SerialiserCollection;

        static Serialisers()
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var dict = new Dictionary<Type, ISerialiser>();
            dict[typeof(bool)] = new ValueTypeSerialiser<bool>(s => new[] { "true", "yes", "1" }.Any(t => t.Equals(s, StringComparison.OrdinalIgnoreCase)), b => b ? "True" : "False");
            dict[typeof(byte)] = new ValueTypeSerialiser<byte>(byte.Parse);
            dict[typeof(short)] = new ValueTypeSerialiser<short>(short.Parse);
            dict[typeof(int)] = new ValueTypeSerialiser<int>(int.Parse);
            dict[typeof(long)] = new ValueTypeSerialiser<long>(long.Parse);
            dict[typeof(ushort)] = new ValueTypeSerialiser<ushort>(ushort.Parse);
            dict[typeof(uint)] = new ValueTypeSerialiser<uint>(uint.Parse);
            dict[typeof(ulong)] = new ValueTypeSerialiser<ulong>(ulong.Parse);
            dict[typeof(float)] = new ValueTypeSerialiser<float>(float.Parse);
            dict[typeof(double)] = new ValueTypeSerialiser<double>(double.Parse);
            dict[typeof(decimal)] = new ValueTypeSerialiser<decimal>(decimal.Parse);
            dict[typeof(string)] = new ValueTypeSerialiser<string>(s => s, asValueAttribue: false);
            dict[typeof(DateTime)] = new ValueTypeSerialiser<DateTime>(DateTime.Parse, d => d.ToString(d.TimeOfDay == TimeSpan.Zero ? "yyyy-MM-dd" : "yyyy-MM-dd HH:mm:ss"), false);
            dict[typeof(TimeSpan)] = new ValueTypeSerialiser<TimeSpan>(TimeSpan.Parse, asValueAttribue: false);
            dict[typeof(AlteryxRecordInfoNet.FieldType)] = new ValueTypeSerialiser<AlteryxRecordInfoNet.FieldType>(
                    t => EnumValues<AlteryxRecordInfoNet.FieldType>().First(v => v.ToString().Substring(5).Replace("_", string.Empty).Equals(t.Replace("_", string.Empty), StringComparison.OrdinalIgnoreCase)),
                    t => t.ToString().Substring(5));

            SerialiserCollection = new ConcurrentDictionary<Type, ISerialiser>(dict);
        }

        /// <summary>
        /// Get Type Serialiser
        /// </summary>
        /// <param name="type">Type to Serialise</param>
        /// <returns>Serialiser for type</returns>
        public static ISerialiser Get(Type type) => SerialiserCollection.GetOrAdd(type, CreateSerialiser);

        private static IEnumerable<T> EnumValues<T>() => Enum.GetValues(typeof(T)).Cast<T>();

        private static ISerialiser CreateSerialiser(Type t)
        {
            if (t.IsEnum)
            {
                return MakeEnumSerialiser(t);
            }

            // XML Serialiser
            // List<T>
            // T[]
            // Call Serialiser<T>

            return null;
        }

        private static ISerialiser MakeEnumSerialiser(Type t)
        {
            var valueTypeSerialiser = typeof(ValueTypeSerialiser<>).MakeGenericType(t);

            var funcHelperType = typeof(FuncWrapper<>).MakeGenericType(t);
            var funcHelper = funcHelperType.GetConstructor(new Type[0])?.Invoke(new object[0]);

            var funcHelperDeserialise = funcHelperType.GetMethod(nameof(FuncWrapper<bool>.Deserialise));
            Func<string, object> func = s => Enum.Parse(t, s, true);
            var typedFunc = funcHelperDeserialise.Invoke(funcHelper, new object[] { func });

            var constructor = valueTypeSerialiser.GetConstructors().First();
            return constructor.Invoke(new[] { typedFunc, null, true }) as ISerialiser;
        }

        private class FuncWrapper<T>
        {
            public Func<string, T> Deserialise(Func<string, object> inner) => s => (T)inner(s);
        }

        private class ValueTypeSerialiser<T> : ISerialiser
        {
            private readonly Func<T, string> _serialise;
            private readonly Func<string, T> _deserialise;
            private readonly bool _asValueAttribue;

            /// <summary>
            /// Initializes a new instance of the <see cref="ValueTypeSerialiser{T}"/> class.
            /// </summary>
            /// <param name="deserialise">Must be provided function to take a string to a value</param>
            /// <param name="serialise">Function allowing custom serialisation otherwise default string is used</param>
            /// <param name="asValueAttribue">Store result as a value attribute</param>
            public ValueTypeSerialiser(Func<string, T> deserialise, Func<T, string> serialise = null, bool asValueAttribue = true)
            {
                this._serialise = serialise ?? (t => t.ToString());
                this._deserialise = deserialise;
                this._asValueAttribue = asValueAttribue;
            }

            /// <summary>
            /// Given an object, serialise to an XmlElement
            /// </summary>
            /// <param name="doc">Parent Xml Document</param>
            /// <param name="name">Name for Node</param>
            /// <param name="value">Object to serialise</param>
            /// <returns>Serialised node</returns>
            public XmlElement Serialise(XmlDocument doc, string name, object value)
            {
                if (!(value is T))
                {
                    return null;
                }

                var serialisedValue = this._serialise((T)value);

                var xmlElement = doc.CreateElement(name);
                if (this._asValueAttribue)
                {
                    var xmlAttribute = doc.CreateAttribute("value");
                    xmlAttribute.InnerText = serialisedValue;
                    xmlElement.Attributes.Append(xmlAttribute);
                }
                else
                {
                    xmlElement.InnerText = serialisedValue;
                }

                return xmlElement;
            }

            /// <summary>
            /// Given an XmlElement, deserialise to an object
            /// </summary>
            /// <param name="node">Serialised node</param>
            /// <returns>Object</returns>
            public object Deserialise(XmlElement node)
                => this._deserialise(node.SelectSingleNode("@value")?.InnerText ?? node.InnerText);
        }
    }
}
