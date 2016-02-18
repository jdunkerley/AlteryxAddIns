namespace JDunkerley.Alteryx
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Linq;

    using AlteryxGuiToolkit.Plugins;

    /// <summary>
    /// Base Tool Class
    /// </summary>
    /// <typeparam name="TConfig">The type of the config object</typeparam>
    /// <typeparam name="TEngine">The type of the engine.</typeparam>
    /// <seealso cref="AlteryxGuiToolkit.Plugins.IPlugin" />
    public abstract class BaseTool<TConfig, TEngine> 
        where TConfig: new()
    {
        private readonly Lazy<Image> _icon;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTool{T, TEngine}"/> class.
        /// </summary>
        protected BaseTool()
        {
            this._icon = new Lazy<Image>(this.GetEmbeddedImage);
        }

        private Image GetEmbeddedImage()
        {
            var ass = this.GetType().Assembly;

            var stream =
                ass.GetManifestResourceNames()
                    .Where(n => n.Contains(this.GetType().Name) && n.EndsWith(".png"))
                    .Select(n => ass.GetManifestResourceStream(n))
                    .FirstOrDefault()
                ?? ass.GetManifestResourceNames()
                       .Where(n => n.Contains("BaseTool") && n.EndsWith(".png"))
                       .Select(n => ass.GetManifestResourceStream(n))
                       .First();

            var bitmap = new Bitmap(stream);
            bitmap.MakeTransparent();
            return bitmap;
        }

        /// <summary>
        /// Get The Icon
        /// </summary>
        /// <returns></returns>
        public Image GetIcon() => _icon.Value;

        /// <summary>
        /// GUI Designer
        /// </summary>
        /// <returns></returns>
        public IPluginConfiguration GetConfigurationGui() => new PropertyGridGUI<TConfig>();

        /// <summary>
        /// Engine Entry Point
        /// </summary>
        /// <returns></returns>
        public EntryPoint GetEngineEntryPoint()
        {
            var entryPoint = new EntryPoint(
                Path.GetFileName(typeof(TEngine).Assembly.Location),
                typeof(TEngine).FullName,
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
