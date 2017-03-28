using System.ComponentModel;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.TypeConverters;

namespace OmniBus
{
    /// <summary>
    ///     Configuration Class for <see cref="HexBinEngine" />
    /// </summary>
    public class HexBinConfig : ConfigWithIncomingConnection
    {
        /// <summary>
        ///     Gets or sets the name of the field for the X co-ordinate
        /// </summary>
        [Category("Output")]
        [Description("Field Name To Use For X Co-ordinate Of HexBin Centre")]
        public string OutputBinXFieldName { get; set; } = "HexBinX";

        /// <summary>
        ///     Gets or sets the name of the field for the Y co-ordinate
        /// </summary>
        [Category("Output")]
        [Description("Field Name To Use For Y Co-ordinate Of HexBin Centre")]
        public string OutputBinYFieldName { get; set; } = "HexBinY";

        /// <summary>
        ///     Gets or sets the name of the field to hash
        /// </summary>
        [Category("Input")]
        [TypeConverter(typeof(InputFieldTypeConverter))]
        [Description("The Field TO Read For The Input Point X Co-Ordinates")]
        [InputPropertyName(
            nameof(HexBinEngine.Input),
            typeof(HexBinEngine),
            FieldType.E_FT_Double,
            FieldType.E_FT_Float,
            FieldType.E_FT_Int16,
            FieldType.E_FT_Int32,
            FieldType.E_FT_Int64)]
        public string InputPointXFieldName { get; set; }

        /// <summary>
        ///     Gets or sets the name of the field to hash
        /// </summary>
        [Category("Input")]
        [TypeConverter(typeof(InputFieldTypeConverter))]
        [Description("The Field TO Read For The Input Point Y Co-Ordinates")]
        [InputPropertyName(
            nameof(HexBinEngine.Input),
            typeof(HexBinEngine),
            FieldType.E_FT_Double,
            FieldType.E_FT_Float,
            FieldType.E_FT_Int16,
            FieldType.E_FT_Int32,
            FieldType.E_FT_Int64)]
        public string InputPointYFieldName { get; set; }

        /// <summary>
        ///     Gets or sets the radius of a hexagon.
        /// </summary>
        public double Radius { get; set; } = 1;

        /// <summary>
        ///     ToString used for annotation
        /// </summary>
        /// <returns>Default Annotation</returns>
        public override string ToString()
        {
            return $"HexBin({this.InputPointXFieldName}, {this.InputPointYFieldName})";
        }
    }
}