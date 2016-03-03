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

    public class HashingTool : BaseTool<HashingTool.Config, HashingTool.Engine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        public enum HashMethod
        {
            MD5,
            RIPEMD160,
            SHA1,
            SHA256,
            SHA384,
            SHA512
        }

        public class Config
        {
            /// <summary>
            /// Gets or sets the name of the output field.
            /// </summary>
            public string OutputFieldName { get; set; } = "HashValue";

            /// <summary>
            /// Gets or sets the name of the input field.
            /// </summary>
            [TypeConverter(typeof(InputFieldTypeConverter))]
            [InputPropertyName(nameof(Engine.Input), typeof(Engine), FieldType.E_FT_String, FieldType.E_FT_V_String, FieldType.E_FT_V_WString, FieldType.E_FT_WString)]
            public string InputFieldName { get; set; }

            /// <summary>
            /// Gets or sets the hash algorithm.
            /// </summary>
            public HashMethod HashAlgorithm { get; set; }

            /// <summary>
            /// ToString used for annotation
            /// </summary>
            /// <returns></returns>
            public override string ToString() => $"{this.InputFieldName}=>{this.OutputFieldName}";

            /// <summary>
            /// Get Algorithm
            /// </summary>
            /// <returns></returns>
            public System.Security.Cryptography.HashAlgorithm GetAlgorithm()
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

            public Engine()
            {
                this.Input = new InputProperty(
                    initFunc: this.InitFunc,
                    progressAction: d => this.Output.UpdateProgress(d),
                    pushFunc: this.PushFunc);
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
                var config = this.GetConfigObject();

                this._inputFieldBase = info.GetFieldByName(config.InputFieldName, false);
                if (this._inputFieldBase == null)
                {
                    return false;
                }

                this._outputRecordInfo = Utilities.CreateRecordInfo(
                    info,
                    new FieldDescription(config.OutputFieldName, FieldType.E_FT_V_String) { Size = 256 });
                this._outputFieldBase = this._outputRecordInfo.GetFieldByName(config.OutputFieldName, false);
                this.Output?.Init(this._outputRecordInfo, nameof(this.Output), null, this.XmlConfig);

                return true;
            }

            private bool PushFunc(RecordData r)
            {
                var record = this._outputRecordInfo.CreateRecord();
                this.Input.Copier.Copy(record, r);

                string input = this._inputFieldBase.GetAsString(r);
                using (var hash = this.GetConfigObject().GetAlgorithm())
                {
                    var bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                    var sb = new StringBuilder();
                    foreach (var b in bytes)
                    {
                        sb.Append(b.ToString("X2"));
                    }
                    this._outputFieldBase.SetFromString(record, sb.ToString());
                }

                this.Output?.PushRecord(record.GetRecord());
                return true;
            }
        }
    }
}