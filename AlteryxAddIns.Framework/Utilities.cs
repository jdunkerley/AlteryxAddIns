namespace JDunkerley.AlteryxAddIns.Framework
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;

    using AlteryxGuiToolkit.Plugins;

    using AlteryxRecordInfoNet;

    using Attributes;

    public static class Utilities
    {
        /// <summary>
        /// Parse Text to DateTime
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static DateTime? ToDateTime(this string text)
        {
            DateTime output;
            if (!DateTime.TryParse(text, out output))
            {
                return null;
            }

            return output;
        }

        /// <summary>
        /// Parse Text to DateTime
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static TimeSpan? ToTimeSpan(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            DateTime output;
            if (!DateTime.TryParse($"1900-01-01 {text}", out output))
            {
                return null;
            }

            return output.TimeOfDay;
        }

        /// <summary>
        /// Gets the incoming connections.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Dictionary<string, PropertyInfo> GetConnections<T>(this Type type)
        {
            var properties =
                type
                    .GetProperties()
                    .Where(p => typeof(T).IsAssignableFrom(p.PropertyType));

            var connections = new Dictionary<string, PropertyInfo>();
            foreach (var property in properties)
            {
                connections[property.Name] = property;
            }

            return connections;
        }

        /// <summary>
        /// Converts ordered set of properties into ALteryx Connections
        /// </summary>
        /// <param name="connections">The connections.</param>
        /// <returns></returns>
        public static IEnumerable<Connection> ToConnections(
            this IEnumerable<KeyValuePair<string, PropertyInfo>> connections)
        {
            return connections
                .OrderBy(kvp => kvp.Value.GetAttrib<OrderingAttribute>()?.Order ?? int.MaxValue)
                .ThenBy(kvp => kvp.Key)
                .Select(
                        kvp =>
                        new Connection(
                            kvp.Key,
                            kvp.Key,
                            false,
                            kvp.Value.GetAttrib<OptionalAttribute>() != null,
                            kvp.Value.GetAttrib<CharLabelAttribute>()?.Label))
                    .ToArray();
        }

        /// <summary>
        /// Gets matching attribute on a property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns></returns>
        public static T GetAttrib<T>(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(typeof(T), true).Cast<T>().FirstOrDefault();
        }

        /// <summary>
        /// Gets matching attribute on a property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="attribsCollection"></param>
        /// <returns></returns>
        public static T GetAttrib<T>(this AttributeCollection attribsCollection)
            where T : class
        {
            T attrib = null;
            foreach (var attribute in attribsCollection)
            {
                attrib = attribute as T;
                if (attrib != null)
                {
                    break;
                }
            }

            return attrib;
        }

        /// <summary>
        /// Add Get or Add Like A Concurrent Dictionary To Dictionaries
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="addFactoryFunc"></param>
        /// <returns></returns>
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> addFactoryFunc)
        {
            TValue output;
            if (dictionary.TryGetValue(key, out output))
            {
                return output;
            }

            dictionary[key] = addFactoryFunc(key);
            return dictionary.GetOrAdd(key, addFactoryFunc);
        }

        public static RecordInfo CreateRecordInfo(
            params FieldDescription[] fields)
        {
            return CreateRecordInfo(null, fields);
        }

        public static RecordInfo CreateRecordInfo(
            RecordInfo inputRecordInfo,
            params FieldDescription[] fields)
        {
            var fieldDict = fields.ToDictionary(f => f.Name, f => false);
            var output = new RecordInfo();

            if (inputRecordInfo != null)
            {
                for (int i = 0; i < inputRecordInfo.NumFields(); i++)
                {
                    var fieldInfo = inputRecordInfo[i];
                    var fieldName = fieldInfo.GetFieldName();

                    if (fieldDict.ContainsKey(fieldName))
                    {
                        fieldDict[fieldName] = true;
                        var descr = fields.First(f => f.Name == fieldName);
                        output.AddField(descr.Name, descr.FieldType, descr.Size, descr.Scale, descr.Source, descr.Description);
                        continue;
                    }

                    output.AddField(fieldInfo);
                }
            }

            foreach (var descr in fields.Where(d => !fieldDict[d.Name]))
            {
                output.AddField(descr.Name, descr.FieldType, descr.Size, descr.Scale, descr.Source, descr.Description);
            }

            return output;
        }

        /// <summary>
        /// Create a FieldDescription Object
        /// </summary>
        /// <returns></returns>
        public static FieldDescription OutputDescription(this OutputType outputType, string fieldName, int size)
        {
            switch (outputType)
            {
                case OutputType.Bool:
                    return new FieldDescription(fieldName, FieldType.E_FT_Bool);
                case OutputType.Byte:
                    return new FieldDescription(fieldName, FieldType.E_FT_Byte);
                case OutputType.Int16:
                    return new FieldDescription(fieldName, FieldType.E_FT_Int16);
                case OutputType.Int32:
                    return new FieldDescription(fieldName, FieldType.E_FT_Int32);
                case OutputType.Int64:
                    return new FieldDescription(fieldName, FieldType.E_FT_Int64);
                case OutputType.Float:
                    return new FieldDescription(fieldName, FieldType.E_FT_Float);
                case OutputType.Double:
                    return new FieldDescription(fieldName, FieldType.E_FT_Double);
                case OutputType.Date:
                    return new FieldDescription(fieldName, FieldType.E_FT_Date);
                case OutputType.DateTime:
                    return new FieldDescription(fieldName, FieldType.E_FT_DateTime);
                case OutputType.Time:
                    return new FieldDescription(fieldName, FieldType.E_FT_Time);
                case OutputType.String:
                    return new FieldDescription(fieldName, FieldType.E_FT_String) { Size = size };
                case OutputType.VString:
                    return new FieldDescription(fieldName, FieldType.E_FT_V_String) { Size = size };
                case OutputType.WString:
                    return new FieldDescription(fieldName, FieldType.E_FT_WString) { Size = size };
                case OutputType.VWString:
                    return new FieldDescription(fieldName, FieldType.E_FT_V_WString) { Size = size };
            }

            return null;
        }
    }
}
