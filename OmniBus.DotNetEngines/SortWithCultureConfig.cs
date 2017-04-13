using System.ComponentModel;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.TypeConverters;

namespace OmniBus
{
    /// <summary>
    ///     Configuration object for <see cref="SortWithCultureEngine" />
    /// </summary>
    public class SortWithCultureConfig : ConfigWithIncomingConnection
    {
        /// <summary>
        ///     Gets or sets the culture.
        /// </summary>
        [TypeConverter(typeof(CultureTypeConverter))]
        [Description("The Culture Used To Sort The Value")]
        public string Culture { get; set; } = CultureTypeConverter.Current;

        /// <summary>
        ///     Gets or sets a value indicating whether to sort with case.
        /// </summary>
        [Description("Sort Ignoring Case")]
        public bool IgnoreCase { get; set; } = false;

        /// <summary>
        ///     Gets or sets the name of the input field.
        /// </summary>
        [Description("The Field On Input Stream To Sort On")]
        [TypeConverter(typeof(InputFieldTypeConverter))]
        [InputPropertyName(
            nameof(SortWithCultureEngine.Input),
            typeof(SortWithCultureEngine),
            FieldType.E_FT_String,
            FieldType.E_FT_V_String,
            FieldType.E_FT_V_WString,
            FieldType.E_FT_WString)]
        public string SortField { get; set; } = "ToSort";

        /// <summary>
        ///     ToString used for annotation
        /// </summary>
        /// <returns>Default annotation for Alteryx to use</returns>
        public override string ToString() => $"Ordered by {this.SortField}";
    }
}