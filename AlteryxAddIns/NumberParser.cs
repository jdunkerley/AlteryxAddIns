namespace JDunkerley.AlteryxAddIns
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    using AlteryxRecordInfoNet;

    using Framework;
    using Framework.Attributes;
    using Framework.ConfigWindows;
    using Framework.Factories;
    using Framework.Interfaces;

    public class NumberParser :
        BaseTool<NumberParser.Config, NumberParser.Engine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        public class Config : ConfigWithIncomingConnection
        {
            public Config()
            {
                this.CultureObject = new Lazy<CultureInfo>(() => CultureTypeConverter.GetCulture(this.Culture));
            }

            /// <summary>
            /// Gets or sets the type of the output.
            /// </summary>
            [Category("Output")]
            [Description("Alteryx Type for the Output Field")]
            [TypeConverter(typeof(FixedListTypeConverter<OutputType>))]
            [FieldList(OutputType.Byte, OutputType.Int16, OutputType.Int32, OutputType.Int64, OutputType.Float, OutputType.Double)]
            public OutputType OutputType { get; set; } = OutputType.Double;

            /// <summary>
            /// Gets or sets the name of the output field.
            /// </summary>
            [Category("Output")]
            [Description("Field Name To Use For Output Field")]
            public string OutputFieldName { get; set; } = "Value";

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
            public string InputFieldName { get; set; } = "ValueInput";

            [Browsable(false)]
            [System.Xml.Serialization.XmlIgnore]
            public Lazy<CultureInfo> CultureObject { get; }

            /// <summary>
            /// ToString used for annotation
            /// </summary>
            /// <returns></returns>
            public override string ToString() => $"{this.InputFieldName} ⇒ {this.OutputFieldName}";
        }

        public class Engine : BaseEngine<Config>
        {
            private IRecordCopier _copier;

            private FieldBase _inputFieldBase;

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
                var fieldDescription = this.ConfigObject?.OutputType.OutputDescription(this.ConfigObject.OutputFieldName, 19);
                if (fieldDescription == null)
                {
                    return false;
                }
                fieldDescription.Source = nameof(NumberParser);
                fieldDescription.Description = $"{this.ConfigObject.InputFieldName} parsed as a number";


                this._inputFieldBase = info.GetFieldByName(this.ConfigObject.InputFieldName, false);
                if (this._inputFieldBase == null)
                {
                    return false;
                }

                this.Output?.Init(Utilities.CreateRecordInfo(info, fieldDescription));
                this._outputFieldBase = this.Output?[this.ConfigObject.OutputFieldName];

                // Create the Copier
                this._copier = this.RecordCopierFactory.CreateCopier(info, this.Output?.RecordInfo, this.ConfigObject.OutputFieldName);

                return true;
            }

            private bool PushFunc(RecordData r)
            {
                var record = this.Output.Record;
                record.Reset();

                this._copier.Copy(record, r);

                string input = this._inputFieldBase.GetAsString(r);

                double value;
                bool result = double.TryParse(input, NumberStyles.Any, this.ConfigObject.CultureObject.Value, out value);

                if (result)
                {
                    this._outputFieldBase.SetFromDouble(record, value);
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