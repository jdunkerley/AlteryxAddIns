using System;
using System.Globalization;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.EventHandlers;
using OmniBus.Framework.Factories;
using OmniBus.Framework.Interfaces;
using OmniBus.Framework.TypeConverters;

namespace OmniBus
{
    /// <summary>
    ///     Engine Class For Parsing A String To A DateTime
    /// </summary>
    public class DateTimeParserEngine : BaseEngine<DateTimeParserConfig>
    {
        private IRecordCopier _copier;
        private FieldBase _inputFieldBase;
        private FieldBase _outputFieldBase;
        private Func<string, DateTime?> _parser;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DateTimeParserEngine" /> class.
        ///     Constructor For Alteryx
        /// </summary>
        public DateTimeParserEngine()
            : this(new RecordCopierFactory(), new InputPropertyFactory(), new OutputHelperFactory())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DateTimeParserEngine" /> class.
        ///     Create An Engine for unit testing.
        /// </summary>
        /// <param name="recordCopierFactory">Factory to create copiers</param>
        /// <param name="inputPropertyFactory">Factory to create input properties</param>
        /// <param name="outputHelperFactory">Factory to create output helpers</param>
        internal DateTimeParserEngine(
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
                source: $"DateTimeParser: {this.ConfigObject.InputFieldName} parsed as a DateTime");

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

            this._parser = this.CreateParser();
            args.Success = true;
        }

        private void OnRecordPushed(IInputProperty sender, RecordPushedEventArgs args)
        {
            this.Output.Record.Reset();

            this._copier.Copy(this.Output.Record, args.RecordData);

            var input = this._inputFieldBase.GetAsString(args.RecordData);
            var result = this._parser(input);

            if (result.HasValue)
            {
                this._outputFieldBase.SetFromString(
                    this.Output.Record,
                    result.Value.ToString(
                        this._outputFieldBase.FieldType == FieldType.E_FT_Time ? "HH:mm:ss" : "yyyy-MM-dd HH:mm:ss"));
            }

            this.Output.Push(this.Output.Record, false, 0);
            args.Success = true;
        }

        private Func<string, DateTime?> CreateParser()
        {
            var format = this.ConfigObject.FormatString;
            var culture = CultureTypeConverter.GetCulture(this.ConfigObject.Culture);

            DateTime dt;
            if (string.IsNullOrWhiteSpace(format))
            {
                return i => DateTime.TryParse(i, culture, DateTimeStyles.AllowWhiteSpaces, out dt)
                                ? (DateTime?)dt
                                : null;
            }

            return i => DateTime.TryParseExact(i, format, culture, DateTimeStyles.AllowWhiteSpaces, out dt)
                            ? (DateTime?)dt
                            : null;
        }
    }
}