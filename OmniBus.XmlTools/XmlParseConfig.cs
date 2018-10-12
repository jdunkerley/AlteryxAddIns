using System.ComponentModel;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.TypeConverters;

namespace OmniBus.XmlTools
{

    public class XmlParseConfig : ConfigWithIncomingConnection
    {
        /// <summary>
        ///     Gets or sets the name of the field to format
        /// </summary>
        [Category("Input")]
        [TypeConverter(typeof(InputFieldTypeConverter))]
        [Description("The Field On Input Stream To Parse XML From")]
        [InputPropertyName(
            nameof(XmlParseEngine.Input),
            typeof(XmlParseEngine),
            FieldType.E_FT_String,
            FieldType.E_FT_V_String,
            FieldType.E_FT_WString,
            FieldType.E_FT_V_WString)]
        public string InputFieldName { get; set; } = "XML";

        /// <summary>
        ///     ToString used for annotation
        /// </summary>
        /// <returns>Default Annotation</returns>
        public override string ToString() => "";
    }
}
