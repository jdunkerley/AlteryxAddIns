using System;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.EventHandlers;
using OmniBus.Framework.Factories;
using OmniBus.Framework.Interfaces;
using OmniBus.Framework.Serialisation;
using OmniBus.Framework.TypeConverters;

namespace OmniBus
{
    /// <summary>
    ///     Formatter Tool Engine
    /// </summary>
    public class StringFormatterEngine : BaseEngine<StringFormatterConfig>
    {
        private IRecordCopier _copier;

        private Func<RecordData, string> _formatter;

        private FieldBase _outputFieldBase;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringFormatterEngine"/> class.
        ///     Constructor For Alteryx
        /// </summary>
        public StringFormatterEngine()
            : base(new OutputHelperFactory())
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
        protected override ISerialiser<StringFormatterConfig> Serialiser() => new Serialiser<StringFormatterConfig>();

        private void OnInit(IInputProperty sender, SuccessEventArgs args)
        {
            // Get Input Field
            var inputFieldBase = this.Input.RecordInfo.GetFieldByName(this.ConfigObject.InputFieldName, false);
            if (inputFieldBase == null)
            {
                args.Success = false;
                return;
            }

            // Create Output Format
            var fieldDescription = new FieldDescription(
                this.ConfigObject.OutputFieldName,
                FieldType.E_FT_V_WString,
                this.ConfigObject.OutputFieldLength,
                source: nameof(StringFormatterEngine));

            var recordInfo = new OmniBus.Framework.Builders.RecordInfoBuilder()
                .AddFieldsFromRecordInfo(this.Input.RecordInfo)
                .AddFields(fieldDescription)
                .Build();

            this.Output?.Init(recordInfo);
            this._outputFieldBase = this.Output?[this.ConfigObject.OutputFieldName];

            // Create the Copier
            this._copier = new OmniBus.Framework.Builders.RecordCopierBuilder(this.Input.RecordInfo, this.Output?.RecordInfo)
                .SkipFields(this.ConfigObject.OutputFieldName)
                .Build();

            // Create the Formatter function
            this._formatter = this.CreateFormatter(inputFieldBase);

            args.Success = this._formatter != null;
        }

        private void OnRecordPushed(IInputProperty sender, RecordPushedEventArgs args)
        {
            var record = this.Output.Record;
            record.Reset();

            this._copier.Copy(record, args.RecordData);

            var result = this._formatter(args.RecordData);

            if (result != null)
            {
                this._outputFieldBase.SetFromString(record, result);
            }
            else
            {
                this._outputFieldBase.SetNull(record);
            }

            this.Output?.Push(record);
        }

        private Func<RecordData, string> CreateFormatter(FieldBase inputFieldBase)
        {
            var format = this.ConfigObject.FormatString;
            var culture = CultureTypeConverter.GetCulture(this.ConfigObject.Culture);

            if (string.IsNullOrWhiteSpace(format))
            {
                return inputFieldBase.GetAsString;
            }

            switch (inputFieldBase.FieldType)
            {
                case FieldType.E_FT_Bool:
                    return r => inputFieldBase.GetAsBool(r)?.ToString(culture);
                case FieldType.E_FT_Byte:
                case FieldType.E_FT_Int16:
                case FieldType.E_FT_Int32:
                case FieldType.E_FT_Int64:
                    return r => inputFieldBase.GetAsInt64(r)?.ToString(format, culture);
                case FieldType.E_FT_Float:
                case FieldType.E_FT_Double:
                case FieldType.E_FT_FixedDecimal:
                    return r => inputFieldBase.GetAsDouble(r)?.ToString(format, culture);
                case FieldType.E_FT_Date:
                case FieldType.E_FT_DateTime:
                    return r => inputFieldBase.GetAsDateTime(r)?.ToString(format, culture);
                case FieldType.E_FT_Time:
                    return r => inputFieldBase.GetAsTimeSpan(r)?.ToString(format, culture);
            }

            return null;
        }
    }
}