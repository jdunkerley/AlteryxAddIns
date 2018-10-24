using System;
using System.Security.Cryptography;
using System.Text;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.EventHandlers;
using OmniBus.Framework.Interfaces;
using OmniBus.Framework.Serialisation;

namespace OmniBus
{
    /// <summary>
    ///     Alteryx Engine For Computing Hash Codes
    /// </summary>
    public class HashCodeGeneratorEngine : BaseEngine<HashCodeGeneratorConfig>
    {
        private HashAlgorithm _hashAlgorithm;
        private FieldBase _inputFieldBase;
        private FieldBase _outputFieldBase;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HashCodeGeneratorEngine" /> class.
        ///     Constructor For Alteryx
        /// </summary>
        public HashCodeGeneratorEngine()
        {
            this.Input = new InputProperty(this);
            this.Input.InitCalled += this.OnInit;
            this.Input.ProgressUpdated += (sender, percentage) => this.Output?.UpdateProgress(percentage, true);
            this.Input.RecordPushed += this.OnRecordPushed;
            this.Input.Closed += sender => this.Output?.Close(true);
        }

        /// <summary>
        ///     Gets the input stream.
        /// </summary>
        [CharLabel('I')]
        public IInputProperty Input { get; }

        /// <summary>
        ///     Gets or sets the output stream.
        /// </summary>
        [CharLabel('O')]
        public IOutputHelper Output { get; set; }

        /// <summary>Create a Serialiser</summary>
        /// <returns><see cref="T:OmniBus.Framework.Serialisation.ISerialiser`1" /> to de-serialise object</returns>
        protected override ISerialiser<HashCodeGeneratorConfig> Serialiser() => new Serialiser<HashCodeGeneratorConfig>();

        private void OnInit(IInputProperty sender, SuccessEventArgs args)
        {
            this._inputFieldBase = this.Input.RecordInfo.GetFieldByName(this.ConfigObject.InputFieldName, false);
            if (this._inputFieldBase == null)
            {
                args.SetFailed();
                return;
            }

            var fieldDescription = new FieldDescription(
                this.ConfigObject.OutputFieldName,
                FieldType.E_FT_V_String,
                256,
                source: nameof(HashCodeGeneratorEngine));

            var recordInfo = new RecordInfoBuilder()
                .AddFields(this.Input.RecordInfo)
                .AddFields(fieldDescription)
                .Build();

            this.Output.Init(recordInfo);
            this._outputFieldBase = this.Output[this.ConfigObject.OutputFieldName];

            this._hashAlgorithm = this.GetAlgorithm();
        }

        private void OnRecordPushed(object sender, RecordData recordData, SuccessEventArgs args)
        {
            var record = this.Output.Record;
            record.Reset();

            this.Input.Copier.Copy(record, recordData);

            var input = this._inputFieldBase.GetAsString(recordData);
            var bytes = this._hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }

            this._outputFieldBase.SetFromString(record, sb.ToString());

            this.Output.Push(record);
        }

        private HashAlgorithm GetAlgorithm()
        {
            switch (this.ConfigObject.HashAlgorithm)
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
                    throw new MissingMethodException($"Can't find method for {this.ConfigObject.HashAlgorithm}");
            }
        }
    }
}