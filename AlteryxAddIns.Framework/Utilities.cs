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
        static Utilities()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                if (args.Name == Assembly.GetAssembly(typeof(Utilities)).FullName)
                {
                    return Assembly.GetAssembly(typeof(Utilities));
                }

                return null;
            };
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
    }
}
