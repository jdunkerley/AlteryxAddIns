using System.ComponentModel;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.TypeConverters;

namespace OmniBus
{
    /// <summary>
    ///     Configuration Class For <see cref="DateTimeParserEngine" />
    /// </summary>
    public class DateTimeParserConfig : ConfigWithIncomingConnection
    {
        /// <summary>
        ///     Gets or sets the type of the output.
        /// </summary>
        [Category("Output")]
        [Description("Alteryx Type for the Output Field")]
        [FieldList(FieldType.E_FT_Date, FieldType.E_FT_DateTime, FieldType.E_FT_Time, FieldType.E_FT_String)]
        [TypeConverter(typeof(FixedListTypeConverter<FieldType>))]
        public FieldType OutputType { get; set; } = FieldType.E_FT_DateTime;

        /// <summary>
        ///     Gets or sets the name of the output field.
        /// </summary>
        [Category("Output")]
        [Description("Field Name To Use For Output Field")]
        public string OutputFieldName { get; set; } = "Date";

        /// <summary>
        ///     Gets or sets the culture.
        /// </summary>
        [TypeConverter(typeof(CultureTypeConverter))]
        [Category("Format")]
        [Description("The Culture Used To Parse The Text Value")]
        public string Culture { get; set; } = CultureTypeConverter.Current;

        /// <summary>
        ///     Gets or sets the name of the input field.
        /// </summary>
        [Category("Input")]
        [Description("The Field On Input Stream To Parse")]
        [TypeConverter(typeof(InputFieldTypeConverter))]
        [InputPropertyName(
            nameof(DateTimeParserEngine.Input),
            typeof(DateTimeParserEngine),
            FieldType.E_FT_String,
            FieldType.E_FT_V_String,
            FieldType.E_FT_V_WString,
            FieldType.E_FT_WString)]
        public string InputFieldName { get; set; } = "DateInput";

        /// <summary>
        ///     Gets or sets the input format.
        /// </summary>
        [Category("Format")]
        [Description("The Format Expected To Parse (blank to use general formats)")]
        public string FormatString { get; set; }

        /// <summary>
        ///     ToString used for annotation
        /// </summary>
        /// <returns>Default Annotation</returns>
        public override string ToString()
        {
            return $"{this.InputFieldName} ({this.FormatString}) ⇒ {this.OutputFieldName}";
        }
    }
}