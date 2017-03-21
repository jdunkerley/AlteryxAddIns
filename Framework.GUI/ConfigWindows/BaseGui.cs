using System.Drawing;
using System.Windows.Forms;
using System.Xml;

using AlteryxGuiToolkit.Plugins;

using OmniBus.Framework.Interfaces;

namespace OmniBus.Framework.ConfigWindows
{
    /// <summary>
    /// Base GUI control handling all the bits for an <see cref="IPluginConfiguration"/>
    /// </summary>
    /// <typeparam name="T">Configuration type, must be constructible</typeparam>
    public abstract class BaseGui<T> : UserControl, IPluginConfiguration
        where T : new()
    {
        /// <summary>
        /// The configuration
        /// </summary>
        private T _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseGui{T}"/> class.
        /// </summary>
        protected BaseGui()
        {
            this.Margin = new Padding(4);
            this.Size = new Size(400, 400);
            this.Name = nameof(PropertyGridGui<T>);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoScaleDimensions = new SizeF(6f, 13f);
        }

        /// <summary>
        /// Gets the current configuration object.
        /// </summary>
        protected T Config => this._config;

        /// <summary>
        /// Gets the configuration control.
        /// </summary>
        /// <param name="docProperties">The document properties.</param>
        /// <param name="eConfig">The current configuration.</param>
        /// <param name="eIncomingMetaInfo">The incoming connection meta data.</param>
        /// <param name="nToolId">The tool identifier.</param>
        /// <param name="strToolName">Name of the tool.</param>
        /// <returns>This object as a control for Alteryx to render.</returns>
        public Control GetConfigurationControl(
            AlteryxGuiToolkit.Document.Properties docProperties,
            XmlElement eConfig,
            XmlElement[] eIncomingMetaInfo,
            int nToolId,
            string strToolName)
        {
            var serialiser = new Serialisation.Serialiser<T>();

            var doc = new XmlDocument();
            doc.LoadXml($"<Config>{eConfig.InnerXml}</Config>");

            this._config = eConfig.InnerText == string.Empty || doc.DocumentElement == null ? new T() : serialiser.Deserialise(doc.DocumentElement);

            if (this._config is IConfigWithIncomingConnection configWithIncomingConnection)
            {
                configWithIncomingConnection.IncomingMetaInfo = eIncomingMetaInfo;
            }

            this.OnObjectSet();

            return this;
        }

        /// <summary>
        /// Saves the results to XML.
        /// </summary>
        /// <param name="eConfig">The e configuration.</param>
        /// <param name="strDefaultAnnotation">The string default annotation.</param>
        public void SaveResultsToXml(XmlElement eConfig, out string strDefaultAnnotation)
        {
            var serialiser = new Serialisation.Serialiser<T>();
            var ser = serialiser.Serialise(this._config);
            eConfig.InnerXml = ser?.InnerXml ?? string.Empty;
            strDefaultAnnotation = this._config.ToString();
        }

        /// <summary>
        /// Add a control to the GUI and make size of the panel
        /// </summary>
        /// <param name="control">Control to add</param>
        protected void AddControl(Control control)
        {
            control.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            control.Size = this.ClientSize;
            this.Controls.Add(control);
        }

        /// <summary>
        /// Called when the <see cref="Config"/> object is set up
        /// </summary>
        protected abstract void OnObjectSet();
    }
}