using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using AlteryxGuiToolkit.Plugins;

namespace JDunkerley.Alteryx
{
    /// <summary>
    /// Simple Date Time Parsing Control
    /// </summary>
    public class DateTimeParser : IPlugin
    {
        private readonly Lazy<Image> _icon =
            new Lazy<Image>(() =>
            {
                var ass = typeof (DateTimeParser).Assembly;

                var stream = ass.GetManifestResourceNames()
                    .Where(n => n.Contains(nameof(DateTimeParser)) && n.EndsWith(".png"))
                    .Select(n => ass.GetManifestResourceStream(n))
                    .First();

                var bitmap = new Bitmap(stream);
                bitmap.MakeTransparent();
                return bitmap;
            });

        /// <summary>
        /// Get The Icon
        /// </summary>
        /// <returns></returns>
        public Image GetIcon() => _icon.Value;

        /// <summary>
        /// GUI Designer
        /// </summary>
        /// <returns></returns>
        public IPluginConfiguration GetConfigurationGui() => new DateTimeParserGui();

        /// <summary>
        /// Engine Entry Point
        /// </summary>
        /// <returns></returns>
        public EntryPoint GetEngineEntryPoint()
        {
            var entryPoint = new EntryPoint(
                Path.GetFileName(typeof(DateTimeParser).Assembly.Location),
                typeof (DateTimeParserEngine).FullName,
                true);
            return entryPoint;
        }

        /// <summary>
        /// Input Connections
        /// </summary>
        /// <returns></returns>
        public Connection[] GetInputConnections() => new Connection[0];

        /// <summary>
        /// Output Connections
        /// </summary>
        /// <returns></returns>
        public Connection[] GetOutputConnections() => new[] { new Connection("output") };
    }
}
