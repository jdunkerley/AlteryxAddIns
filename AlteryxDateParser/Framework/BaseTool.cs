namespace JDunkerley.Alteryx.Framework
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Linq;

    using AlteryxGuiToolkit.Plugins;

    using AlteryxRecordInfoNet;

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

        private readonly Connection[] _inputConnections;

        private readonly Connection[] _outputConnections;


        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTool{T, TEngine}"/> class.
        /// </summary>
        protected BaseTool()
        {
            this._icon = new Lazy<Image>(this.GetEmbeddedImage);

            // Read Incoming Connection Nodes
            this._inputConnections =
                typeof(TEngine).GetConnections<IIncomingConnectionInterface>().ToConnections().ToArray();


            // Read Outgoing Connection Nodes
            this._outputConnections =
                typeof(TEngine).GetConnections<PluginOutputConnectionHelper>().ToConnections().ToArray();
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
                       .Where(n => n.Contains(this._inputConnections.Length == 0 ? "BaseInput" : "BaseTool") && n.EndsWith(".png"))
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
        public Image GetIcon() => this._icon.Value;

        /// <summary>
        /// GUI Designer
        /// </summary>
        /// <returns></returns>
        public IPluginConfiguration GetConfigurationGui() => new PropertyGridGui<TConfig>();

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
        public Connection[] GetInputConnections() => this._inputConnections;

        /// <summary>
        /// Output Connections
        /// </summary>
        /// <returns></returns>
        public Connection[] GetOutputConnections() => this._outputConnections;
    }
}
