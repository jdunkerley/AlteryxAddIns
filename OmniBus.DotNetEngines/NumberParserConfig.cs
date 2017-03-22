using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.TypeConverters;

namespace OmniBus
{
    /// <summary>
    /// Configuration object or <see cref="NumberParserEngine"/>
    /// </summary>
    public class NumberParserConfig : ConfigWithIncomingConnection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumberParserConfig"/> class.
        /// </summary>
        public NumberParserConfig()
        {
            this.CultureObject = new Lazy<CultureInfo>(() => CultureTypeConverter.GetCulture(this.Culture));
        }

        /// <summary>
        ///     Gets or sets the type of the output.
        /// </summary>
        [Category("Output")]
        [Description("Alteryx Type for the Output Field")]
        [FieldList(
            FieldType.E_FT_Byte,
            FieldType.E_FT_Int16,
            FieldType.E_FT_Int32,
            FieldType.E_FT_Int64,
            FieldType.E_FT_Float,
            FieldType.E_FT_Double)]
        [TypeConverter(typeof(FixedListTypeConverter<FieldType>))]
        public FieldType OutputType { get; set; } = FieldType.E_FT_Double;

        /// <summary>
        ///     Gets or sets the name of the output field.
        /// </summary>
        [Category("Output")]
        [Description("Field Name To Use For Output Field")]
        public string OutputFieldName { get; set; } = "Value";

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
            nameof(NumberParserEngine.Input),
            typeof(NumberParserEngine),
            FieldType.E_FT_String,
            FieldType.E_FT_V_String,
            FieldType.E_FT_V_WString,
            FieldType.E_FT_WString)]
        public string InputFieldName { get; set; } = "ValueInput";

        /// <summary>
        /// Gets a configured <see cref="CultureInfo"/> Object For Parsing
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public Lazy<CultureInfo> CultureObject { get; }

        /// <summary>
        ///     ToString used for annotation
        /// </summary>
        /// <returns>Default Annotation</returns>
        public override string ToString() => $"{this.InputFieldName} ⇒ {this.OutputFieldName}";
    }
}