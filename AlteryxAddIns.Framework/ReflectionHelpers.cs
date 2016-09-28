namespace JDunkerley.AlteryxAddIns.Framework
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;

    using AlteryxGuiToolkit.Plugins;

    using Attributes;

    /// <summary>
    /// Set of extension methods for working with reflection.
    /// </summary>
    public static class ReflectionHelpers
    {
        static ReflectionHelpers()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {
                    if (args.Name == Assembly.GetAssembly(typeof(ReflectionHelpers)).FullName)
                    {
                        return Assembly.GetAssembly(typeof(ReflectionHelpers));
                    }

                    return null;
                };
        }

        /// <summary>
        /// Gets all properties of a <see cref="Type"/> which can be assigned to a <typeparamref name="T"/>.
        /// </summary>
        /// <param name="type">The type to read properties from.</param>
        /// <typeparam name="T">The type for the properties.</typeparam>
        /// <returns>Dictionary of properties for the type</returns>
        public static Dictionary<string, PropertyInfo> GetProperties<T>(this Type type)
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
        /// Converts ordered set of properties into Alteryx Connections
        /// </summary>
        /// <param name="connections">The connections.</param>
        /// <returns>Set of Alteryx Connection objects in the specified order with labels and names set.</returns>
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
        /// <typeparam name="TAttrib">Attribute type to find.</typeparam>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>Attribute or null if not found.</returns>
        internal static TAttrib GetAttrib<TAttrib>(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(typeof(TAttrib), true).Cast<TAttrib>().FirstOrDefault();
        }

        /// <summary>
        /// Gets matching attribute in an attribute collection.
        /// </summary>
        /// <typeparam name="TAttrib">Attribute type to find.</typeparam>
        /// <param name="attribsCollection">Collection of attributes to scan.</param>
        /// <returns>Attribute or null if not found.</returns>
        internal static TAttrib GetAttrib<TAttrib>(this AttributeCollection attribsCollection)
            where TAttrib : class
        {
            foreach (var attribute in attribsCollection)
            {
                var attrib = attribute as TAttrib;
                if (attrib != null)
                {
                    return attrib;
                }
            }

            return null;
        }
    }
}