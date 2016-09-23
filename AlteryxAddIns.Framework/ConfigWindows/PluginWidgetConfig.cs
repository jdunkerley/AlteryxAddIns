namespace JDunkerley.AlteryxAddIns.Framework.ConfigWindows
{
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    /// <summary>
    /// Plug In Widget Based Configuration Screen
    /// Based on Group Boxes
    /// </summary>
    /// <typeparam name="TConfig">Configuration Data Type To serialize / De-serialize</typeparam>
    public class PluginWidgetConfig<TConfig> : UserControl, AlteryxGuiToolkit.Plugins.IPluginConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginWidgetConfig{TConfig}"/> class.
        /// </summary>
        public PluginWidgetConfig()
        {
            this.Margin = new Padding(4);
            this.Size = new Size(400, 400);
            this.Name = nameof(PluginWidgetConfig<TConfig>);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoScaleDimensions = new SizeF(6f, 13f);

            var type = typeof(TConfig);

            var props =
                type.GetProperties()
                    .OrderBy(p => p.GetAttrib<CategoryAttribute>()?.Category ?? "ZZZZ")
                    .ThenBy(p => p.Name);

            var categoriesBoxes = new ConcurrentDictionary<string, GroupBox>();

            foreach (var propertyInfo in props)
            {
                var category = propertyInfo.GetAttrib<CategoryAttribute>()?.Category ?? "General";
                var groupBox = categoriesBoxes.GetOrAdd(
                    category,
                    c => PluginWidgetHelper.CreateGroupBox(c).AddAtTop(this.Controls));

                // Add A Label
                PluginWidgetHelper.CreateLabel(propertyInfo).AddAtTop(groupBox.Controls);
                PluginWidgetHelper.GetControl(propertyInfo).AddAtTop(groupBox.Controls);
            }
        }

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
            System.Xml.XmlElement eConfig,
            System.Xml.XmlElement[] eIncomingMetaInfo,
            int nToolId,
            string strToolName)
        {
            return this;
        }

        /// <summary>
        /// Saves the results to XML.
        /// </summary>
        /// <param name="eConfig">The e configuration.</param>
        /// <param name="strDefaultAnnotation">The string default annotation.</param>
        public void SaveResultsToXml(System.Xml.XmlElement eConfig, out string strDefaultAnnotation)
        {
            strDefaultAnnotation = "Under development...";
        }
    }
}