namespace JDunkerley.AlteryxAddIns
{
    using System;
    using System.ComponentModel;
    using System.Security.Cryptography;
    using System.Text;

    using AlteryxRecordInfoNet;

    using Framework;
    using Framework.Attributes;
    using Framework.ConfigWindows;
    using Framework.Factories;
    using Framework.Interfaces;

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

        public class Config : ConfigWithIncomingConnection
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

            private FieldBase _outputFieldBase;

            private HashAlgorithm _hashAlgorithm;

            /// <summary>
            /// Constructor For Alteryx
            /// </summary>
            public Engine()
                : this(new RecordCopierFactory(), new InputPropertyFactory())
            {
            }

            /// <summary>
            /// Create An Engine
            /// </summary>
            /// <param name="recordCopierFactory">Factory to create copiers</param>
            /// <param name="inputPropertyFactory">Factory to create input properties</param>
            internal Engine(IRecordCopierFactory recordCopierFactory, IInputPropertyFactory inputPropertyFactory)
                : base(recordCopierFactory)
            {
                this.Input = inputPropertyFactory.Build(recordCopierFactory, this.ShowDebugMessages);
                this.Input.InitCalled += this.OnInit;
                this.Input.ProgressUpdated += (sender, args) => this.Output.UpdateProgress(args.Progress, true);
                this.Input.RecordPushed += this.OnRecordPushed;
                this.Input.Closed += (sender, args) => this.Output?.Close(true);
            }

            /// <summary>
            /// Gets the input stream.
            /// </summary>
            [CharLabel('I')]
            public IInputProperty Input { get; }

            /// <summary>
            /// Gets or sets the output stream.
            /// </summary>
            [CharLabel('O')]
            public OutputHelper Output { get; set; }

            private void OnInit(object sender, SuccessEventArgs args)
            {
                this._inputFieldBase = this.Input.RecordInfo.GetFieldByName(this.ConfigObject.InputFieldName, false);
                if (this._inputFieldBase == null)
                {
                    args.Success = false;
                    return;
                }

                this.Output.Init(
                    FieldDescription.CreateRecordInfo(
                    this.Input.RecordInfo,
                    new FieldDescription(this.ConfigObject.OutputFieldName, FieldType.E_FT_V_String) { Size = 256, Source = nameof(HashCodeGenerator)}));
                this._outputFieldBase = this.Output[this.ConfigObject.OutputFieldName];

                this._hashAlgorithm = this.ConfigObject.GetAlgorithm();

                args.Success = true;
            }

            private void OnRecordPushed(object sender, RecordPushedEventArgs args)
            {
                var record = this.Output.Record;
                record.Reset();

                this.Input.Copier.Copy(record, args.RecordData);

                string input = this._inputFieldBase.GetAsString(args.RecordData);
                var bytes = this._hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder();
                foreach (var b in bytes)
                {
                    sb.Append(b.ToString("X2"));
                }
                this._outputFieldBase.SetFromString(record, sb.ToString());

                this.Output.Push(record);
                args.Success = true;
            }
        }
    }
}