namespace JDunkerley.AlteryxAddIns
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    using AlteryxGuiToolkit.Plugins;

    using AlteryxRecordInfoNet;

    using Framework;
    using Framework.Attributes;
    using Framework.ConfigWindows;
    using Framework.Interfaces;

    using Framework.Factories;

    [PlugInGroup("JDTools", "DateTime Parser")]
    public class DateTimeParser :
        BaseTool<DateTimeParser.Config, DateTimeParser.Engine>, IPlugin
    {
        public class Config
        {
            /// <summary>
            /// Gets or sets the type of the output.
            /// </summary>
            [Category("Output")]
            [Description("Alteryx Type for the Output Field")]
            [TypeConverter(typeof(FixedListTypeConverter<OutputType>))]
            [FieldList(OutputType.Date, OutputType.DateTime, OutputType.Time, OutputType.String)]
            public OutputType OutputType { get; set; } = OutputType.DateTime;

            /// <summary>
            /// Gets or sets the name of the output field.
            /// </summary>
            [Category("Output")]
            [Description("Field Name To Use For Output Field")]
            public string OutputFieldName { get; set; } = "Date";

            /// <summary>
            /// Gets or sets the culture.
            /// </summary>
            [TypeConverter(typeof(CultureTypeConverter))]
            [Category("Format")]
            [Description("The Culture Used To Parse The Text Value")]
            public string Culture { get; set; } = CultureTypeConverter.Current;

            /// <summary>
            /// Gets or sets the name of the input field.
            /// </summary>
            [Category("Input")]
            [Description("The Field On Input Stream To Parse")]
            [TypeConverter(typeof(InputFieldTypeConverter))]
            [InputPropertyName(nameof(Engine.Input), typeof(Engine), FieldType.E_FT_String, FieldType.E_FT_V_String, FieldType.E_FT_V_WString, FieldType.E_FT_WString)]
            public string InputFieldName { get; set; } = "DateInput";

            /// <summary>
            /// Gets or sets the input format.
            /// </summary>
            [Category("Format")]
            [Description("The Format Expected To Parse (blank to use general formats)")]
            public string FormatString { get; set; }

            /// <summary>
            /// ToString used for annotation
            /// </summary>
            /// <returns></returns>
            public override string ToString() => $"{this.InputFieldName} ({this.FormatString}) ⇒ {this.OutputFieldName}";

            /// <summary>
            /// Create Parser Func
            /// </summary>
            /// <returns></returns>
            public Func<string, DateTime?> CreateParser()
            {
                var format = this.FormatString;
                var culture = CultureTypeConverter.GetCulture(this.Culture);

                DateTime dt;
                if (string.IsNullOrWhiteSpace(format))
                {
                    return i => DateTime.TryParse(i, culture, DateTimeStyles.AllowWhiteSpaces, out dt)
                    ? (DateTime?)dt
                    : null;
                }

                return input => DateTime.TryParseExact(input, format, culture, DateTimeStyles.AllowWhiteSpaces, out dt)
                    ? (DateTime?)dt
                    : null;
            }
        }

        public class Engine : BaseEngine<Config>
        {
            private FieldBase _inputFieldBase;

            private IRecordCopier _copier;

            private Func<string, DateTime?> _parser;

            private FieldBase _outputFieldBase;

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
                this.Input.InitCalled += (sender, args) => args.Success = this.InitFunc(this.Input.RecordInfo);
                this.Input.ProgressUpdated += (sender, args) => this.Output?.UpdateProgress(args.Progress, true);
                this.Input.RecordPushed += (sender, args) => args.Success = this.PushFunc(args.RecordData);
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

            private bool InitFunc(RecordInfo info)
            {
                var fieldDescription = this.ConfigObject.OutputType.OutputDescription(this.ConfigObject.OutputFieldName, 19);
                if (fieldDescription == null)
                {
                    return false;
                }
                fieldDescription.Source = nameof(DateTimeParser);
                fieldDescription.Description = $"{this.ConfigObject.InputFieldName} parsed as a DateTime";


                this._inputFieldBase = this.Input.RecordInfo.GetFieldByName(this.ConfigObject.InputFieldName, false);
                if (this._inputFieldBase == null)
                {
                    return false;
                }

                this.Output?.Init(Utilities.CreateRecordInfo(info, fieldDescription));
                this._outputFieldBase = this.Output?[this.ConfigObject.OutputFieldName];

                // Create the Copier
                this._copier = this.RecordCopierFactory.CreateCopier(this.Input.RecordInfo, this.Output?.RecordInfo, this.ConfigObject.OutputFieldName);

                this._parser = this.ConfigObject.CreateParser();
                return true;
            }

            private bool PushFunc(RecordData r)
            {
                var record = this.Output.Record;
                record.Reset();

                this._copier.Copy(record, r);

                string input = this._inputFieldBase.GetAsString(r);
                var result = this._parser(input);

                if (result.HasValue)
                {
                    this._outputFieldBase.SetFromString(record, result.Value.ToString(this._outputFieldBase.FieldType == FieldType.E_FT_Time ? "HH:mm:ss" : "yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    this._outputFieldBase.SetNull(record);
                }

                this.Output.Push(record);
                return true;
            }
        }
    }
}
