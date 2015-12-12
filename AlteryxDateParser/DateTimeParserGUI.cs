using System;
using System.Windows.Forms;
using System.Xml;
using AlteryxGuiToolkit.Plugins;

namespace JDunkerley.Alteryx
{
    public class DateTimeParserGui : UserControl, IPluginConfiguration
    {
        private PropertyGrid propGrid;

        public DateTimeParserGui()
        {
            this.Margin = new Padding(4);
            this.Size = new System.Drawing.Size(520, 530);

            var panel = new Panel { Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right, Size = this.ClientSize };
            this.Controls.Add(panel);
            propGrid = new PropertyGrid { Dock = DockStyle.Fill };
            panel.Controls.Add(propGrid);

            this.ParentChanged += (sender, args) =>
            {
                this.Size = this.Parent.ClientSize;
                this.Controls[0].Size = this.ClientSize;

                Parent.SizeChanged += (o, eventArgs) =>
                {
                    this.Size = this.Parent.ClientSize;
                };
            };
        }

        public Control GetConfigurationControl(
            AlteryxGuiToolkit.Document.Properties docProperties,
            XmlElement eConfig,
            XmlElement[] eIncomingMetaInfo,
            int nToolId,
            string strToolName)
        {
            var config = new DateTimeParserEngine.Config();
            this.propGrid.SelectedObject = config;

            return this;
        }

        public void SaveResultsToXml(XmlElement eConfig, out string strDefaultAnnotation)
        {
            // No Config Yet
            strDefaultAnnotation = "JDDemo";
        }
    }
}