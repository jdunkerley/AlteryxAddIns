using System;
using System.ComponentModel;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.TypeConverters;

namespace OmniBus
{
    /// <summary>
    ///     Configuration object for the Formatter Tool
    /// </summary>
    public class StringFormatterConfig : ConfigWithIncomingConnection
    {
        /// <summary>
        ///     Specify the name of the formatted field in the Output
        /// </summary>
        [Category("Output")]
        [Description("Field Name To Use For Output Field")]
        public string OutputFieldName { get; set; } = "FormattedValue";

        /// <summary>
        ///     Specify the length of the Output field
        /// </summary>
        [Category("Output")]
        public int OutputFieldLength { get; set; } = 64;

        /// <summary>
        ///     Specify the culture to use for formatting the value
        /// </summary>
        [Category("Format")]
        [TypeConverter(typeof(CultureTypeConverter))]
        [Description("The Culture Used To Format The Value")]
        public string Culture { get; set; } = CultureTypeConverter.Current;

        /// <summary>
        ///     Specify the name of the field to format
        /// </summary>
        [Category("Input")]
        [TypeConverter(typeof(InputFieldTypeConverter))]
        [Description("The Field On Input Stream To Format")]
        [InputPropertyName(nameof(StringFormatterEngine.Input), typeof(StringFormatterEngine), FieldType.E_FT_Bool, FieldType.E_FT_Byte,
            FieldType.E_FT_Int16, FieldType.E_FT_Int32, FieldType.E_FT_Int64, FieldType.E_FT_Float,
            FieldType.E_FT_Double, FieldType.E_FT_FixedDecimal, FieldType.E_FT_Date, FieldType.E_FT_DateTime,
            FieldType.E_FT_Time)]
        public string InputFieldName { get; set; } = "Value";

        /// <summary>
        ///     Specify the format to be applied
        /// </summary>
        [Category("Format")]
        [Description("The Format String To Use (blank to use default)")]
        public string FormatString { get; set; }

        /// <summary>
        ///     ToString used for annotation
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{this.InputFieldName}=>{this.OutputFieldName} [{this.FormatString}]";

        /// <summary>
        ///     Create Formatter <see cref="Func{TResult}" /> Delegate
        /// </summary>
        /// <param name="inputFieldBase"></param>
        /// <returns></returns>
        public Func<RecordData, string> CreateFormatter(FieldBase inputFieldBase)
        {
            var format = this.FormatString;
            var culture = CultureTypeConverter.GetCulture(this.Culture);

            if (string.IsNullOrWhiteSpace(format))
            {
                return inputFieldBase.GetAsString;
            }

            switch (inputFieldBase.FieldType)
            {
                case FieldType.E_FT_Bool:
                    return r => inputFieldBase.GetAsBool(r)?.ToString(culture);
                case FieldType.E_FT_Byte:
                case FieldType.E_FT_Int16:
                case FieldType.E_FT_Int32:
                case FieldType.E_FT_Int64:
                    return r => inputFieldBase.GetAsInt64(r)?.ToString(format, culture);
                case FieldType.E_FT_Float:
                case FieldType.E_FT_Double:
                case FieldType.E_FT_FixedDecimal:
                    return r => inputFieldBase.GetAsDouble(r)?.ToString(format, culture);
                case FieldType.E_FT_Date:
                case FieldType.E_FT_DateTime:
                    return r => inputFieldBase.GetAsDateTime(r)?.ToString(format, culture);
                case FieldType.E_FT_Time:
                    return r => inputFieldBase.GetAsTimeSpan(r)?.ToString(format, culture);
            }

            return null;
        }
    }
}