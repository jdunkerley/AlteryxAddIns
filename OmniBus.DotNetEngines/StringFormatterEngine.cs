using System;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.EventHandlers;
using OmniBus.Framework.Factories;
using OmniBus.Framework.Interfaces;

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
        ///     Constructor For Alteryx
        /// </summary>
        public StringFormatterEngine()
            : this(new RecordCopierFactory(), new InputPropertyFactory(), new OutputHelperFactory())
        {
        }

        /// <summary>
        ///     Create An Engine for unit testing.
        /// </summary>
        /// <param name="recordCopierFactory">Factory to create copiers</param>
        /// <param name="inputPropertyFactory">Factory to create input properties</param>
        /// <param name="outputHelperFactory">Factory to create output helpers</param>
        internal StringFormatterEngine(
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
            // Get Input Field
            var inputFieldBase = this.Input.RecordInfo.GetFieldByName(this.ConfigObject.InputFieldName, false);
            if (inputFieldBase == null)
            {
                args.Success = false;
                return;
            }

            // Create Output Format
            var fieldDescription = new FieldDescription(this.ConfigObject.OutputFieldName, FieldType.E_FT_V_WString)
                                       {
                                           Size
                                               =
                                               this
                                                   .ConfigObject
                                                   .OutputFieldLength,
                                           Source
                                               =
                                               nameof
                                               (
                                                   StringFormatterEngine)
                                       };
            this.Output?.Init(FieldDescription.CreateRecordInfo(this.Input.RecordInfo, fieldDescription));
            this._outputFieldBase = this.Output?[this.ConfigObject.OutputFieldName];

            // Create the Copier
            this._copier = this.RecordCopierFactory.CreateCopier(
                this.Input.RecordInfo,
                this.Output?.RecordInfo,
                this.ConfigObject.OutputFieldName);

            // Create the Formatter function
            this._formatter = this.ConfigObject.CreateFormatter(inputFieldBase);

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
    }
}