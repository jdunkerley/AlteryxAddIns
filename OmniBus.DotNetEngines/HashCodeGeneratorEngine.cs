using System.Security.Cryptography;
using System.Text;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.EventHandlers;
using OmniBus.Framework.Factories;
using OmniBus.Framework.Interfaces;

namespace OmniBus
{
    /// <summary>
    /// Alteryx Engine For Computing Hash Codes
    /// </summary>
    public class HashCodeGeneratorEngine : BaseEngine<HashCodeGeneratorConfig>
    {
        private HashAlgorithm _hashAlgorithm;

        private FieldBase _inputFieldBase;

        private FieldBase _outputFieldBase;

        /// <summary>
        ///     Constructor For Alteryx
        /// </summary>
        public HashCodeGeneratorEngine()
            : this(new RecordCopierFactory(), new InputPropertyFactory(), new OutputHelperFactory())
        {
        }

        /// <summary>
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

        private void OnInit(IInputProperty sender, SuccessEventArgs args)
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
                    new FieldDescription(this.ConfigObject.OutputFieldName, FieldType.E_FT_V_String)
                        {
                            Size = 256,
                            Source = nameof(HashCodeGeneratorEngine).Replace("Engine", "")
                        }));
            this._outputFieldBase = this.Output[this.ConfigObject.OutputFieldName];

            this._hashAlgorithm = this.ConfigObject.GetAlgorithm();

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
    }
}