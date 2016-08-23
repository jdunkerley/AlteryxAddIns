namespace JDunkerley.AlteryxAddIns.Framework.ConfigWindows
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Windows.Forms;

    using AlteryxGuiToolkit.PluginWidgets.Data;

    using Attributes;

    public static class PluginWidgetHelper
    {
        public static T AddAtTop<T>(this Control.ControlCollection collection, T control)
            where T : Control
        {
            collection.Add(control);
            collection.SetChildIndex(control, 0);
            return control;
        }

        public static GroupBox GroupBox(string text)
        {
            return new GroupBox
                       {
                           AutoSize = true,
                           AutoSizeMode = AutoSizeMode.GrowOnly,
                           Dock = DockStyle.Top,
                           Text = text,
                           Padding = new Padding(3)
                       };
        }

        public static Label CreateLabel(PropertyInfo propertyInfo)
        {
            return new Label
                       {
                           Dock = DockStyle.Top,
                           AutoSize = true,
                           Text = propertyInfo.GetAttrib<DescriptionAttribute>()?.Description ?? propertyInfo.Name
                       };
        }

        public static Control GetControl(PropertyInfo propertyInfo)
        {
            var fieldList = propertyInfo.GetAttrib<FieldListAttribute>();
            if (fieldList != null)
            {
                return CreateRadioButtons(propertyInfo.Name, fieldList.OrderedDictionary);
            }

            return new Label() { Text = "Not Implemented" };
        }

        public static Panel CreateRadioButtons(string xmlName, IEnumerable<KeyValuePair<string, object>> values)
        {
            var panel = new Panel();

            foreach (var kvp in values)
            {
                var enumControl = new AlteryxGuiToolkit.PluginWidgets.UI.StringSelectorRadioButton
                                      {
                                          Text = kvp.Key,
                                          Tag = kvp.Value,
                                          XmlString = xmlName
                                      };

                panel.Controls.AddAtTop(enumControl);
            }

            return panel;
        }

        public static void BindToData(this Control.ControlCollection controls, Manager manager)
        {
            foreach (Control control in controls)
            {
                control.Controls.BindToData(manager);
            }
        }
    }
}