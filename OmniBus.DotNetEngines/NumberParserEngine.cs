using System.Globalization;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.EventHandlers;
using OmniBus.Framework.Factories;
using OmniBus.Framework.Interfaces;

namespace OmniBus
{
    /// <summary>
    /// Configuration class for <see cref="NumberParserEngine"/>
    /// </summary>
    public class NumberParserEngine : BaseEngine<NumberParserConfig>
    {
        private IRecordCopier _copier;

        private FieldBase _inputFieldBase;

        private FieldBase _outputFieldBase;

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberParserEngine"/> class.
        /// Constructor For Alteryx
        /// </summary>
        public NumberParserEngine()
            : this(new RecordCopierFactory(), new InputPropertyFactory(), new OutputHelperFactory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberParserEngine"/> class.
        /// Create An NumberParserEngine for unit testing.
        /// </summary>
        /// <param name="recordCopierFactory">Factory to create copiers</param>
        /// <param name="inputPropertyFactory">Factory to create input properties</param>
        /// <param name="outputHelperFactory">Factory to create output helpers</param>
        internal NumberParserEngine(
            IRecordCopierFactory recordCopierFactory,
            IInputPropertyFactory inputPropertyFactory,
            IOutputHelperFactory outputHelperFactory)
            : base(recordCopierFactory, outputHelperFactory)
        {
            this.Input = inputPropertyFactory.Build(recordCopierFactory, this.ShowDebugMessages);
            this.Input.InitCalled += this.OnInit;
            this.Input.ProgressUpdated += (sender, args) => this.Output?.UpdateProgress(args.Progress, true);
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
            var fieldDescription = new FieldDescription(
                this.ConfigObject.OutputFieldName,
                this.ConfigObject.OutputType,
                source: $"NumberParser: {this.ConfigObject.InputFieldName} parsed as a number");

            this._inputFieldBase = this.Input.RecordInfo.GetFieldByName(this.ConfigObject.InputFieldName, false);
            if (this._inputFieldBase == null)
            {
                args.Success = false;
                return;
            }

            this.Output?.Init(FieldDescription.CreateRecordInfo(this.Input.RecordInfo, fieldDescription));
            this._outputFieldBase = this.Output?[this.ConfigObject.OutputFieldName];

            // Create the Copier
            this._copier = this.RecordCopierFactory.CreateCopier(
                this.Input.RecordInfo,
                this.Output?.RecordInfo,
                this.ConfigObject.OutputFieldName);

            args.Success = true;
        }

        private void OnRecordPushed(object sender, RecordPushedEventArgs args)
        {
            var record = this.Output.Record;
            record.Reset();

            this._copier.Copy(record, args.RecordData);

            var input = this._inputFieldBase.GetAsString(args.RecordData);

            if (double.TryParse(input, NumberStyles.Any, this.ConfigObject.CultureObject.Value, out double value))
            {
                this._outputFieldBase.SetFromDouble(record, value);
            }

            this.Output.Push(record);
            args.Success = true;
        }
    }
}