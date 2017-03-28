using System.ComponentModel;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.TypeConverters;

namespace OmniBus
{
    /// <summary>
    /// Configuration Class For <see cref="HashCodeGeneratorEngine"/>
    /// </summary>
    public class HashCodeGeneratorConfig : ConfigWithIncomingConnection
    {
        /// <summary>
        ///     Gets or sets the name of the hashed value field in the Output
        /// </summary>
        [Category("Output")]
        [Description("Field Name To Use For Output Field")]
        public string OutputFieldName { get; set; } = "HashValue";

        /// <summary>
        ///     Gets or sets the name of the field to hash
        /// </summary>
        [Category("Input")]
        [TypeConverter(typeof(InputFieldTypeConverter))]
        [Description("The Field On Input Stream To Hash")]
        [InputPropertyName(
            nameof(HashCodeGeneratorEngine.Input),
            typeof(HashCodeGeneratorEngine),
            FieldType.E_FT_String,
            FieldType.E_FT_V_String,
            FieldType.E_FT_V_WString,
            FieldType.E_FT_WString)]
        public string InputFieldName { get; set; }

        /// <summary>
        ///     Gets or sets the method used the hash the value
        /// </summary>
        [Description("The Hashing Algorithm To Use")]
        public HashCodeGeneratorMethod HashAlgorithm { get; set; }

        /// <summary>
        ///     ToString used for annotation
        /// </summary>
        /// <returns>Defualt Annotation</returns>
        public override string ToString() => $"{this.InputFieldName}=>{this.HashAlgorithm}({this.OutputFieldName})";
    }
}