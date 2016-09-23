namespace JDunkerley.AlteryxAddIns.Framework.ConfigWindows
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Reflection;
    using System.Windows.Forms;

    using AlteryxGuiToolkit.PluginWidgets.Data;

    using Attributes;

    /// <summary>
    /// Helper class for working with Alteryx's Plug in Widgets
    /// </summary>
    public static class PluginWidgetHelper
    {
        /// <summary>
        /// Add A Control to the top of a collection
        /// </summary>
        /// <typeparam name="T">Control type</typeparam>
        /// <param name="control">Control to add</param>
        /// <param name="collection">Control collection to add it to</param>
        /// <returns>Collection allowing you to chain them together</returns>
        public static T AddAtTop<T>(this T control, Control.ControlCollection collection)
            where T : Control
        {
            collection.Add(control);
            collection.SetChildIndex(control, 0);
            return control;
        }

        /// <summary>
        /// For a property on a configuration object create a control
        /// </summary>
        /// <param name="propertyInfo">Property</param>
        /// <returns>A control creating a configuration setting for a property</returns>
        public static Control GetControl(PropertyInfo propertyInfo)
        {
            var fieldList = propertyInfo.GetAttrib<FieldListAttribute>();
            if (fieldList != null)
            {
                return CreateRadioButtons(propertyInfo.Name, fieldList.OrderedDictionary);
            }

            return CreateLabel("Not Implemented");
        }

        /// <summary>
        /// Create a CreateGroupBox
        /// </summary>
        /// <param name="text">Label for the group</param>
        /// <returns>New group box control</returns>
        public static GroupBox CreateGroupBox(string text)
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

        /// <summary>
        /// Create a label for a property.
        /// Uses the Description if present, or name otherwise.
        /// </summary>
        /// <param name="propertyInfo">Property</param>
        /// <returns>Label control</returns>
        public static Label CreateLabel(PropertyInfo propertyInfo)
            => CreateLabel(propertyInfo.GetAttrib<DescriptionAttribute>()?.Description ?? propertyInfo.Name);

        /// <summary>
        /// Create a docked label
        /// </summary>
        /// <param name="text">Text for the label</param>
        /// <returns>Label control</returns>
        public static Label CreateLabel(string text)
        {
            return new Label
                       {
                           Dock = DockStyle.Top,
                           AutoSize = true,
                           Text = text,
                           TextAlign = ContentAlignment.MiddleLeft,
                           Padding = new Padding(3)
                       };
        }

        /// <summary>
        /// Creates a Radio button selector for a fixed list of options
        /// </summary>
        /// <param name="xmlName">XML Property Name</param>
        /// <param name="values">Set of Values</param>
        /// <returns>Panel control containing the Radio buttons</returns>
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

                enumControl.AddAtTop(panel.Controls);
            }

            return panel;
        }

        /// <summary>
        /// Bind a control to a Data Manager
        /// </summary>
        /// <param name="controls">Control collection to Bind</param>
        /// <param name="manager">Data Manager</param>
        public static void BindToData(this Control.ControlCollection controls, Manager manager)
        {
            foreach (Control control in controls)
            {
                control.Controls.BindToData(manager);
            }
        }
    }
}