using System;
using System.ComponentModel;

namespace OmniBus.XmlTools
{
    /// <summary>Configuration Class for <see cref="XmlInputEngine"/></summary>
    public class XmlInputConfig
    {
       /// <summary>
        /// Gets or sets the file name containing the Xml
        /// </summary>
        [Description("Specify the filename of the XML file")]
        [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string FileName { get; set; }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object, used as the Default Annotation for Alteryx.</returns>
        public override string ToString()

        {
            try
            {
                return string.IsNullOrWhiteSpace(this.FileName) ? string.Empty : System.IO.Path.GetFileName(this.FileName);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
