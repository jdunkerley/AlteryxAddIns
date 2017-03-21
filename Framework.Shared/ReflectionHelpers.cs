using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OmniBus.Framework
{
    /// <summary>
    ///     Set of extension methods for working with reflection.
    /// </summary>
    public static class ReflectionHelpers
    {
        static ReflectionHelpers()
        {
            AppDomain.CurrentDomain.AssemblyResolve +=
                (sender, args) =>
                    args.Name == Assembly.GetAssembly(typeof(ReflectionHelpers)).FullName
                        ? Assembly.GetAssembly(typeof(ReflectionHelpers))
                        : null;
        }

        /// <summary>
        ///     Gets all properties of a <see cref="Type" /> which can be assigned to a <typeparamref name="T" />.
        /// </summary>
        /// <param name="type">The type to read properties from.</param>
        /// <typeparam name="T">The type for the properties.</typeparam>
        /// <returns>Dictionary of properties for the type</returns>
        public static Dictionary<string, PropertyInfo> GetProperties<T>(this Type type)
        {
            var properties = type.GetProperties().Where(p => typeof(T).IsAssignableFrom(p.PropertyType));
            return properties.ToDictionary(p => p.Name, p => p);
        }
    }
}