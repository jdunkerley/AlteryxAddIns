using System.ComponentModel;

using AlteryxRecordInfoNet;

using OmniBus.Framework.Attributes;
using OmniBus.Framework.TypeConverters;

namespace OmniBus
{
    /// <summary>
    /// Configuration Object For <see cref="DateTimeInputEngine"/>
    /// </summary>
    public class DateTimeInputConfig
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
        ///     Gets or sets the Date To Return
        /// </summary>
        [Description("Value to Return")]
        public DateTimeInputValueToReturn DateToReturn { get; set; } = DateTimeInputValueToReturn.Now;

        /// <summary>
        ///     ToString used for annotation
        /// </summary>
        /// <returns>Default Annotation for the tool</returns>
        public override string ToString() => $"{this.OutputFieldName}={this.DateToReturn}";
    }
}