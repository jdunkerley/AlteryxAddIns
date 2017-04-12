using System;
using System.Security.Cryptography;
using System.Text;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.EventHandlers;
using OmniBus.Framework.Factories;
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
            : this(new RecordCopierFactory(), new InputPropertyFactory(), new OutputHelperFactory())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HashCodeGeneratorEngine" /> class.
        ///     Create An Engine for unit testing.
        /// </summary>
        /// <param name="recordCopierFactory">Factory to create copiers</param>
        /// <param name="inputPropertyFactory">Factory to create input properties</param>
        /// <param name="outputHelperFactory">Factory to create output helpers</param>
        internal HashCodeGeneratorEngine(
            IRecordCopierFactory recordCopierFactory,
            IInputPropertyFactory inputPropertyFactory,
            IOutputHelperFactory outputHelperFactory)
            : base(recordCopierFactory, outputHelperFactory)
        {
            this.Input = inputPropertyFactory.Build(recordCopierFactory, this.ShowDebugMessages);
            this.Input.InitCalled += this.OnInit;
            this.Input.ProgressUpdated += (sender, args) => this.Output.UpdateProgress(args.Progress, true);
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
                args.Success = false;
                return;
            }

            var field = new FieldDescription(
                this.ConfigObject.OutputFieldName,
                FieldType.E_FT_V_String,
                256,
                source: nameof(HashCodeGeneratorEngine));

            this.Output.Init(FieldDescription.CreateRecordInfo(this.Input.RecordInfo, field));
            this._outputFieldBase = this.Output[this.ConfigObject.OutputFieldName];

            this._hashAlgorithm = this.GetAlgorithm();

            args.Success = true;
        }

        private void OnRecordPushed(object sender, RecordPushedEventArgs args)
        {
            var record = this.Output.Record;
            record.Reset();

            this.Input.Copier.Copy(record, args.RecordData);

            var input = this._inputFieldBase.GetAsString(args.RecordData);
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