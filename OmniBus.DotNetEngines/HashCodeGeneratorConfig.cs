using System;
using System.ComponentModel;
using System.Security.Cryptography;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.TypeConverters;

namespace OmniBus
{
    /// <summary>
    /// Configuration Object For <see cref="HashCodeGeneratorEngine"/>
    /// </summary>
    public class HashCodeGeneratorConfig : ConfigWithIncomingConnection
    {
        /// <summary>
        ///     Specify the name of the  hashed value field in the Output
        /// </summary>
        [Category("Output")]
        [Description("Field Name To Use For Output Field")]
        public string OutputFieldName { get; set; } = "HashValue";

        /// <summary>
        ///     Specify the name of the field to hash
        /// </summary>
        [Category("Input")]
        [TypeConverter(typeof(InputFieldTypeConverter))]
        [Description("The Field On Input Stream To Hash")]
        [InputPropertyName(nameof(HashCodeGeneratorEngine.Input), typeof(HashCodeGeneratorEngine), FieldType.E_FT_String, FieldType.E_FT_V_String,
            FieldType.E_FT_V_WString, FieldType.E_FT_WString)]
        public string InputFieldName { get; set; }

        /// <summary>
        ///     Specify the method used the hash the value
        /// </summary>
        [Description("The Hashing Algorithm To Use")]
        public HashCodeGeneratorMethod HashAlgorithm { get; set; }

        /// <summary>
        ///     ToString used for annotation
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{this.InputFieldName}=>{this.HashAlgorithm}({this.OutputFieldName})";

        /// <summary>
        ///     Get Algorithm
        /// </summary>
        /// <returns></returns>
        public HashAlgorithm GetAlgorithm()
        {
            switch (this.HashAlgorithm)
            {
                case HashCodeGeneratorMethod.MD5:
                    return new MD5Cng();
                case HashCodeGeneratorMethod.RIPEMD160:
                    return new RIPEMD160Managed();
                case HashCodeGeneratorMethod.SHA1:
                    return new SHA1Managed();
                case HashCodeGeneratorMethod.SHA256:
                    return new SHA256Managed();
                case HashCodeGeneratorMethod.SHA384:
                    return new SHA384Managed();
                case HashCodeGeneratorMethod.SHA512:
                    return new SHA512Managed();
                default:
                    throw new MissingMethodException($"Can't find method for {this.HashAlgorithm}");
            }
        }
    }
}