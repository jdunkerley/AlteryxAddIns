namespace JDunkerley.Alteryx.Framework
{
    using System;
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
        where T: new()
    {
        /// <summary>
        /// The _property grid
        /// </summary>
        private readonly PropertyGrid _propertyGrid;

        /// <summary>
        /// The old parent.
        /// </summary>
        private Control _currentParent;

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
            this.Size = new System.Drawing.Size(520, 530);

            this._propertyGrid = new PropertyGrid { Dock = DockStyle.Fill };
            this.Controls.Add(this._propertyGrid);

            this.ParentChanged += this.OnParentChanged;
        }

        private void OnParentChanged(object sender, EventArgs args)
        {
            if (this._currentParent != null)
            {
                this._currentParent.SizeChanged -= this.OnParentOnSizeChanged;
            }

            this._currentParent = this.Parent;
            this.SetSize();
            this.Parent.SizeChanged += this.OnParentOnSizeChanged;
        }

        private void OnParentOnSizeChanged(object o, EventArgs eventArgs)
        {
            this.SetSize();
        }

        private void SetSize()
        {
            this.Size = this.Parent.ClientSize;
            this.Controls[0].Size = this.ClientSize;
        }

        /// <summary>
        /// Gets the configuration control.
        /// </summary>
        /// <param name="docProperties">The document properties.</param>
        /// <param name="eConfig">The e configuration.</param>
        /// <param name="eIncomingMetaInfo">The e incoming meta information.</param>
        /// <param name="nToolId">The n tool identifier.</param>
        /// <param name="strToolName">Name of the string tool.</param>
        /// <returns></returns>
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

            this._config = eConfig.InnerText == "" || doc.DocumentElement == null
                ? new T() 
                : (T)serialiser.Deserialize(new XmlNodeReader(doc.DocumentElement));

            this._propertyGrid.SelectedObject = this._config;
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

            eConfig.InnerXml = doc.DocumentElement?.InnerXml ?? "";
            strDefaultAnnotation = this._config.ToString();
        }
    }
}