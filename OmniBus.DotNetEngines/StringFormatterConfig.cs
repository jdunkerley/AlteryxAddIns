using System.ComponentModel;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.TypeConverters;

namespace OmniBus
{
    /// <summary>
    ///     Configuration class for the Formatter Tool
    /// </summary>
    public class StringFormatterConfig : ConfigWithIncomingConnection
    {
        /// <summary>
        ///     Gets or sets the name of the formatted field in the Output
        /// </summary>
        [Category("Output")]
        [Description("Field Name To Use For Output Field")]
        public string OutputFieldName { get; set; } = "FormattedValue";

        /// <summary>
        ///     Gets or sets the length of the Output field
        /// </summary>
        [Category("Output")]
        public int OutputFieldLength { get; set; } = 64;

        /// <summary>
        ///     Gets or sets the culture to use for formatting the value
        /// </summary>
        [Category("Format")]
        [TypeConverter(typeof(CultureTypeConverter))]
        [Description("The Culture Used To Format The Value")]
        public string Culture { get; set; } = CultureTypeConverter.Current;

        /// <summary>
        ///     Gets or sets the name of the field to format
        /// </summary>
        [Category("Input")]
        [TypeConverter(typeof(InputFieldTypeConverter))]
        [Description("The Field On Input Stream To Format")]
        [InputPropertyName(
            nameof(StringFormatterEngine.Input),
            typeof(StringFormatterEngine),
            FieldType.E_FT_Bool,
            FieldType.E_FT_Byte,
            FieldType.E_FT_Int16,
            FieldType.E_FT_Int32,
            FieldType.E_FT_Int64,
            FieldType.E_FT_Float,
            FieldType.E_FT_Double,
            FieldType.E_FT_FixedDecimal,
            FieldType.E_FT_Date,
            FieldType.E_FT_DateTime,
            FieldType.E_FT_Time)]
        public string InputFieldName { get; set; } = "Value";

        /// <summary>
        ///     Gets or sets the format to be applied
        /// </summary>
        [Category("Format")]
        [Description("The Format String To Use (blank to use default)")]
        public string FormatString { get; set; }

        /// <summary>
        ///     ToString used for annotation
        /// </summary>
        /// <returns>Default Annotation</returns>
        public override string ToString()
        {
            return $"{this.InputFieldName}=>{this.OutputFieldName} [{this.FormatString}]";
        }
    }
}