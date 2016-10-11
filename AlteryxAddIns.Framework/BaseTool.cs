namespace JDunkerley.AlteryxAddIns.Framework
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Linq;

    using AlteryxGuiToolkit.Plugins;

    using ConfigWindows;

    /// <summary>
    /// Base Tool Class
    /// </summary>
    /// <typeparam name="TConfig">The type of the configuration object</typeparam>
    /// <typeparam name="TEngine">The type of the engine.</typeparam>
    /// <seealso cref="AlteryxGuiToolkit.Plugins.IPlugin" />
    public abstract class BaseTool<TConfig, TEngine>
        where TConfig : new()
        where TEngine : AlteryxRecordInfoNet.INetPlugin
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
                typeof(TEngine).GetProperties<AlteryxRecordInfoNet.IIncomingConnectionInterface>().ToConnections().ToArray();

            // Read Outgoing Connection Nodes
            this._outputConnections =
                typeof(TEngine).GetProperties<OutputHelper>().ToConnections().ToArray();
        }

        /// <summary>
        /// Get The Icon
        /// </summary>
        /// <returns>Icon for Alteryx to use to represent the tool.</returns>
        public Image GetIcon() => this._icon.Value;

        /// <summary>
        /// GUI Designer
        /// </summary>
        /// <returns>The configuration object to render in the properties window.</returns>
        public virtual IPluginConfiguration GetConfigurationGui() => new PropertyGridGui<TConfig>();

        /// <summary>
        /// Engine Entry Point
        /// </summary>
        /// <returns>An entry point to the function for the Engine to run.</returns>
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
        /// <returns>List of input connections for a tool</returns>
        public Connection[] GetInputConnections() => this._inputConnections;

        /// <summary>
        /// Output Connections
        /// </summary>
        /// <returns>List of output connections for a tool</returns>
        public Connection[] GetOutputConnections() => this._outputConnections;

        private Image GetEmbeddedImage()
        {
            var assembly = this.GetType().Assembly;
            var frameworkAssembly = typeof(FieldDescription).Assembly;

            var stream =
                assembly.GetManifestResourceNames()
                    .Where(n => n.Contains(this.GetType().Name))
                    .Select(n => assembly.GetManifestResourceStream(n))
                    .FirstOrDefault()
                ?? frameworkAssembly.GetManifestResourceNames()
                       .Where(n => n.Contains(this._inputConnections.Length == 0 ? "BaseInput" : "BaseTool") && n.EndsWith(".png"))
                       .Select(n => frameworkAssembly.GetManifestResourceStream(n))
                       .First();

            var bitmap = new Bitmap(stream);
            bitmap.MakeTransparent();
            return bitmap;
        }
    }
}