using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

using AlteryxGuiToolkit.Plugins;

using OmniBus.Framework.Attributes;

namespace OmniBus.Framework
{
    /// <summary>
    /// Helper for the <see cref="BaseTool{TConfig,TEngine}"/>
    /// </summary>
    internal static class Helpers
    {
        private static readonly Lazy<Image> BaseInputLazy;

        private static readonly Lazy<Image> BaseToolLazy;

        static Helpers()
        {
            var frameworkAssembly = typeof(Helpers).Assembly;
            BaseInputLazy = new Lazy<Image>(() => GetImageFromAssembly(frameworkAssembly, nameof(BaseInput)));
            BaseToolLazy = new Lazy<Image>(() => GetImageFromAssembly(frameworkAssembly, nameof(BaseTool)));
        }

        /// <summary>
        /// Gets The Default Image For Input Tools
        /// </summary>
        internal static Image BaseInput => BaseInputLazy.Value;

        /// <summary>
        /// Gets The Default Image For General Tools
        /// </summary>
        internal static Image BaseTool => BaseToolLazy.Value;

        /// <summary>
        /// Read An Embedded Image From An Assembly
        /// </summary>
        /// <param name="assembly">Assembly To Read</param>
        /// <param name="name">Name of Image to Read</param>
        /// <returns>Image If Found Or NULL</returns>
        internal static Image GetImageFromAssembly(Assembly assembly, string name)
        {
            var stream =
                assembly.GetManifestResourceNames()
                    .Where(n => n.Contains(name.Replace("Engine", string.Empty)))
                    .Select(assembly.GetManifestResourceStream)
                    .FirstOrDefault();
            if (stream == null)
            {
                return null;
            }

            var bitmap = new Bitmap(stream);
            bitmap.MakeTransparent();
            return bitmap;
        }

        /// <summary>
        /// Convert PropertyInfo dictionary to connections
        /// </summary>
        /// <param name="connections">Set of PropertyInfo</param>
        /// <returns>Alteryx Connections</returns>
        internal static IEnumerable<Connection> ToConnections(
            IEnumerable<KeyValuePair<string, PropertyInfo>> connections)
        {
            return connections
                .OrderBy(kvp => kvp.Value.GetCustomAttribute<OrderingAttribute>()?.Order ?? int.MaxValue)
                .ThenBy(kvp => kvp.Key)
                .Select(
                    kvp =>
                        new Connection(
                            kvp.Key,
                            kvp.Key,
                            false,
                            kvp.Value.GetCustomAttribute<OptionalAttribute>() != null,
                            kvp.Value.GetCustomAttribute<CharLabelAttribute>()?.Label))
                .ToArray();
        }
    }
}