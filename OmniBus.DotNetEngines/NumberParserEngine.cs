using System.Globalization;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.EventHandlers;
using OmniBus.Framework.Interfaces;
using OmniBus.Framework.Serialisation;

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
        protected override ISerialiser<NumberParserConfig> Serialiser() => new Serialiser<NumberParserConfig>();

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

            var recordInfo = new RecordInfoBuilder()
                .AddFields(this.Input.RecordInfo)
                .ReplaceFields(fieldDescription)
                .Build();

            this.Output?.Init(recordInfo);
            this._outputFieldBase = this.Output?[this.ConfigObject.OutputFieldName];

            // Create the Copier
            this._copier = new RecordCopierBuilder(this.Input.RecordInfo, this.Output?.RecordInfo)
                .SkipFields(this.ConfigObject.OutputFieldName)
                .Build();

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