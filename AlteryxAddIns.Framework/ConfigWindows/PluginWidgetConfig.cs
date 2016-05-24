    namespace JDunkerley.AlteryxAddIns.Framework.ConfigWindows
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    /// <summary>
    /// Plugin Widget Based Configuration Screen
    /// Based on Group Boxes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PluginWidgetConfig<T> : UserControl, AlteryxGuiToolkit.Plugins.IPluginConfiguration
    {
        public PluginWidgetConfig()
        {
            this.Margin = new Padding(4);
            this.Size = new Size(400, 400);
            this.Name = nameof(PluginWidgetConfig<T>);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoScaleDimensions = new SizeF(6f, 13f);

            var type = typeof(T);

            var props =
                type.GetProperties()
                    .OrderBy(p => p.GetAttrib<CategoryAttribute>()?.Category ?? "ZZZZ")
                    .ThenBy(p => p.Name);

            var categoriesBoxes = new Dictionary<string, GroupBox>();

            foreach (var propertyInfo in props)
            {
                var category = propertyInfo.GetAttrib<CategoryAttribute>()?.Category ?? "General";
                var groupBox = categoriesBoxes.GetOrAdd(
                    category,
                    c => this.Controls.AddAtTop(PluginWidgetHelper.GroupBox(c)));

                // Add A Label
                groupBox.Controls.AddAtTop(PluginWidgetHelper.CreateLabel(propertyInfo));
                groupBox.Controls.AddAtTop(PluginWidgetHelper.GetControl(propertyInfo));
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="docProperties"></param>
        /// <param name="eConfig"></param>
        /// <param name="eIncomingMetaInfo"></param>
        /// <param name="nToolId"></param>
        /// <param name="strToolName"></param>
        /// <returns></returns>
        public Control GetConfigurationControl(
            AlteryxGuiToolkit.Document.Properties docProperties,
            System.Xml.XmlElement eConfig,
            System.Xml.XmlElement[] eIncomingMetaInfo,
            int nToolId,
            string strToolName)
        {
            return this;
        }

        public void SaveResultsToXml(System.Xml.XmlElement eConfig, out string strDefaultAnnotation)
        {
            strDefaultAnnotation = "Undevelopement...";
        }
    }
}