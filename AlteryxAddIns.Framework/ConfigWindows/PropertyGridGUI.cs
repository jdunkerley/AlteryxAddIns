namespace JDunkerley.AlteryxAddIns.Framework.ConfigWindows
{
    using System.Drawing;
    using System.Windows.Forms;
    using System.Xml;

    using AlteryxGuiToolkit.Plugins;

    /// <summary>
    /// Simple Property Grid Based Configuration Panel
    /// </summary>
    /// <typeparam name="T">Configuration Object</typeparam>
    /// <seealso cref="System.Windows.Forms.UserControl" />
    /// <seealso cref="AlteryxGuiToolkit.Plugins.IPluginConfiguration" />
    public class PropertyGridGui<T> : UserControl, IPluginConfiguration
        where T : new()
    {
        /// <summary>
        /// The _property grid
        /// </summary>
        private readonly PropertyGrid _propertyGrid;

        /// <summary>
        /// The configuration
        /// </summary>
        private T _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGridGui{T}"/> class.
        /// </summary>
        public PropertyGridGui()
        {
            this.Margin = new Padding(4);
            this.Size = new Size(400, 400);
            this.Name = nameof(PropertyGridGui<T>);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoScaleDimensions = new SizeF(6f, 13f);

            this._propertyGrid = new PropertyGrid
                                     {
                                         Anchor =
                                             AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                                         PropertySort = PropertySort.CategorizedAlphabetical,
                                         Size = this.ClientSize
                                     };
            this.Controls.Add(this._propertyGrid);
        }

        /// <summary>
        /// Gets the configuration control.
        /// </summary>
        /// <param name="docProperties">The document properties.</param>
        /// <param name="eConfig">The e configuration.</param>
        /// <param name="eIncomingMetaInfo">The e incoming meta information.</param>
        /// <param name="nToolId">The n tool identifier.</param>
        /// <param name="strToolName">Name of the string tool.</param>
        /// <returns>This object as a control for Alteryx to render.</returns>
        public Control GetConfigurationControl(
            AlteryxGuiToolkit.Document.Properties docProperties,
            XmlElement eConfig,
            XmlElement[] eIncomingMetaInfo,
            int nToolId,
            string strToolName)
        {
            var serialiser = new System.Xml.Serialization.XmlSerializer(typeof(T));

            var doc = new XmlDocument();
            doc.LoadXml($"<Config>{eConfig.InnerXml}</Config>");

            this._config = eConfig.InnerText == string.Empty || doc.DocumentElement == null
                ? new T()
                : (T)serialiser.Deserialize(new XmlNodeReader(doc.DocumentElement));

            this._propertyGrid.SelectedObject = this._config;
            Statics.CurrentMetaData = eIncomingMetaInfo;
            return this;
        }

        /// <summary>
        /// Saves the results to XML.
        /// </summary>
        /// <param name="eConfig">The e configuration.</param>
        /// <param name="strDefaultAnnotation">The string default annotation.</param>
        public void SaveResultsToXml(XmlElement eConfig, out string strDefaultAnnotation)
        {
            var doc = new XmlDocument();
            var serialiser = new System.Xml.Serialization.XmlSerializer(typeof(T));
            using (XmlWriter writer = doc.CreateNavigator().AppendChild())
            {
                serialiser.Serialize(writer, this._config);
            }

            eConfig.InnerXml = doc.DocumentElement?.InnerXml ?? string.Empty;
            strDefaultAnnotation = this._config.ToString();
        }
    }
}