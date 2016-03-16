namespace JDunkerley.AlteryxAddins
{
    using System;
    using System.ComponentModel;
    using System.Security.Cryptography;
    using System.Text;

    using AlteryxRecordInfoNet;

    using JDunkerley.AlteryxAddIns.Framework;
    using JDunkerley.AlteryxAddIns.Framework.Attributes;
    using JDunkerley.AlteryxAddIns.Framework.ConfigWindows;

    public class HashCodeGenerator : BaseTool<HashCodeGenerator.Config, HashCodeGenerator.Engine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        // ReSharper disable InconsistentNaming
        public enum HashMethod
        {
            MD5,
            RIPEMD160,
            SHA1,
            SHA256,
            SHA384,
            SHA512
        }
        // ReSharper restore InconsistentNaming

        public class Config
        {
            /// <summary>
            /// Specify the name of the  hashed value field in the Output
            /// </summary>
            [Category("Output")]

            [Description("Field Name To Use For Output Field")]
            public string OutputFieldName { get; set; } = "HashValue";

            /// <summary>
            /// Specify the name of the field to hash
            /// </summary>
            [Category("Input")]
            [TypeConverter(typeof(InputFieldTypeConverter))]
            [Description("The Field On Input Stream To Hash")]
            [InputPropertyName(nameof(Engine.Input), typeof(Engine), FieldType.E_FT_String, FieldType.E_FT_V_String, FieldType.E_FT_V_WString, FieldType.E_FT_WString)]
            public string InputFieldName { get; set; }

            /// <summary>
            /// Specify the method used the hash the value
            /// </summary>
            [Description("The Hashing Algorithm To Use")]
            public HashMethod HashAlgorithm { get; set; }

            /// <summary>
            /// ToString used for annotation
            /// </summary>
            /// <returns></returns>
            public override string ToString() => $"{this.InputFieldName}=>{this.HashAlgorithm}({this.OutputFieldName})";

            /// <summary>
            /// Get Algorithm
            /// </summary>
            /// <returns></returns>
            public HashAlgorithm GetAlgorithm()
            {
                switch (this.HashAlgorithm)
                {
                    case HashMethod.MD5:
                        return new MD5Cng();
                    case HashMethod.RIPEMD160:
                        return new RIPEMD160Managed();
                    case HashMethod.SHA1:
                        return new SHA1Managed();
                    case HashMethod.SHA256:
                        return new SHA256Managed();
                    case HashMethod.SHA384:
                        return new SHA384Managed();
                    case HashMethod.SHA512:
                        return new SHA512Managed();
                    default:
                        throw new MissingMethodException($"Can't find method for {this.HashAlgorithm}");
                }
            }
        }

        public class Engine : BaseEngine<Config>
        {
            private FieldBase _inputFieldBase;

            private RecordInfo _outputRecordInfo;

            private FieldBase _outputFieldBase;

            private HashAlgorithm _hashAlgorithm;

            public Engine()
            {
                this.Input = new InputProperty(
                    initFunc: this.InitFunc,
                    progressAction: d =>
                        {
                            this.Output.UpdateProgress(d);
                            this.Engine.OutputToolProgress(this.NToolId, d);
                        },
                    pushFunc: this.PushFunc,
                    closedAction: () =>
                        {
                            this._hashAlgorithm = null;
                            this.Output?.Close();
                        });
            }

            /// <summary>
            /// Gets the input stream.
            /// </summary>
            [CharLabel('I')]
            public InputProperty Input { get; }

            /// <summary>
            /// Gets or sets the output stream.
            /// </summary>
            [CharLabel('O')]
            public PluginOutputConnectionHelper Output { get; set; }

            private bool InitFunc(RecordInfo info)
            {
                this._inputFieldBase = info.GetFieldByName(this.ConfigObject.InputFieldName, false);
                if (this._inputFieldBase == null)
                {
                    return false;
                }

                this._outputRecordInfo = Utilities.CreateRecordInfo(
                    info,
                    new FieldDescription(this.ConfigObject.OutputFieldName, FieldType.E_FT_V_String) { Size = 256, Source = nameof(HashCodeGenerator)});
                this._outputFieldBase = this._outputRecordInfo.GetFieldByName(this.ConfigObject.OutputFieldName, false);
                this.Output?.Init(this._outputRecordInfo, nameof(this.Output), null, this.XmlConfig);

                this._hashAlgorithm = this.ConfigObject.GetAlgorithm();

                return true;
            }

            private bool PushFunc(RecordData r)
            {
                var record = this._outputRecordInfo.CreateRecord();
                this.Input.Copier.Copy(record, r);

                string input = this._inputFieldBase.GetAsString(r);
                var bytes = this._hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder();
                foreach (var b in bytes)
                {
                    sb.Append(b.ToString("X2"));
                }
                this._outputFieldBase.SetFromString(record, sb.ToString());

                this.Output?.PushRecord(record.GetRecord());
                return true;
            }
        }
    }
}