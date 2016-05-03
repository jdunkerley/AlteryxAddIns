namespace JDunkerley.AlteryxAddins
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Forms;

    using JDunkerley.AlteryxAddIns.Framework;

    public class PluginWidgetConfig<T> : System.Windows.Forms.UserControl, AlteryxGuiToolkit.Plugins.IPluginConfiguration
    {
        private readonly Dictionary<string, GroupBox> _categoriesBoxes;

        /// <summary>
        /// The old parent.
        /// </summary>
        private Control _currentParent;

        public PluginWidgetConfig()
        {
            this.Padding = new Padding(5);

            var type = typeof(T);

            var props =
                type.GetProperties()
                    .OrderBy(p => p.GetAttrib<CategoryAttribute>()?.Category ?? "ZZZZ")
                    .ThenBy(p => p.Name);

            this._categoriesBoxes = new Dictionary<string, GroupBox>();

            foreach (var propertyInfo in props)
            {
                var category = propertyInfo.GetAttrib<CategoryAttribute>()?.Category ?? "General";
                var groupBox = this._categoriesBoxes.GetOrAdd(category, this.CreateGroupBox);

                // Add A Label
                var label = new Label { Dock = DockStyle.Top, AutoSize = true, Text = propertyInfo.GetAttrib<DescriptionAttribute>()?.Description ?? propertyInfo.Name };
                groupBox.Controls.Add(label);
                groupBox.Controls.SetChildIndex(label, 0);

                if (propertyInfo.PropertyType.IsEnum)
                {
                    // Add an Enum
                    var panel = new Panel();
                    foreach (var name in Enum.GetNames(propertyInfo.PropertyType))
                    {
                        var enumControl = new AlteryxGuiToolkit.PluginWidgets.UI.StringSelectorRadioButton();
                        enumControl.Text = name;
                        enumControl.XmlString = name;

                        panel.Controls.Add(enumControl);
                        panel.Controls.SetChildIndex(enumControl, 0);
                    }

                    groupBox.Controls.Add(panel);
                    groupBox.Controls.SetChildIndex(panel, 0);
                }

                //AlteryxGuiToolkit.PluginWidgets.UI.StyleConfigUI
            }

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

            foreach (Control control in this.Controls)
            {
                control.Width = this.ClientSize.Width - 10;
            }
            ;
        }

        private GroupBox CreateGroupBox(string category)
        {
            var groupBox = new GroupBox()
                               {
                                   AutoSize = true,
                                   AutoSizeMode = AutoSizeMode.GrowOnly,
                                   Dock = DockStyle.Top,
                                   Text = category,
                                   Padding = new Padding(3)
                               };
            this.Controls.Add(groupBox);
            this.Controls.SetChildIndex(groupBox, 0);
            return groupBox;
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
        public System.Windows.Forms.Control GetConfigurationControl(
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