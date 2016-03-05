namespace JDunkerley.AlteryxAddins
{
    using System.ComponentModel;
    using System.Globalization;

    using AlteryxRecordInfoNet;

    using JDunkerley.AlteryxAddIns.Framework;
    using JDunkerley.AlteryxAddIns.Framework.Attributes;
    using JDunkerley.AlteryxAddIns.Framework.ConfigWindows;

    public class NumericParserTool :
        BaseTool<NumericParserTool.Config, NumericParserTool.Engine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        public class Config
        {
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
            [InputPropertyName(nameof(DateTimeParserTool.Engine.Input), typeof(DateTimeParserTool.Engine), FieldType.E_FT_String, FieldType.E_FT_V_String, FieldType.E_FT_V_WString, FieldType.E_FT_WString)]
            public string InputFieldName { get; set; } = "ValueInput";

            /// <summary>
            /// ToString used for annotation
            /// </summary>
            /// <returns></returns>
            public override string ToString() => $"{this.InputFieldName} ⇒ {this.OutputFieldName}";
        }

        public class Engine : BaseEngine<DateTimeParserTool.Config>
        {
            private FieldBase _inputFieldBase;

            private RecordCopier _copier;

            private RecordInfo _outputRecordInfo;

            private FieldBase _outputFieldBase;

            private CultureInfo _culture;

            public Engine()
            {
                this.Input = new InputProperty(
                    initFunc: this.InitFunc,
                    progressAction: d => this.Output.UpdateProgress(d),
                    pushFunc: this.PushFunc);
            }

            /// <summary>
            /// Gets the input stream.
            /// </summary>
            [CharLabel('I')]
            public InputProperty Input { get; }

            /// <summary>
            /// Gets or sets the output stream.
            /// </summary>
            [CharLabel('O')]
            public PluginOutputConnectionHelper Output { get; set; }

            private bool InitFunc(RecordInfo info)
            {
                var config = this.GetConfigObject();

                var fieldDescription = config?.OutputType.OutputDescription(config.OutputFieldName, 19);
                if (fieldDescription == null)
                {
                    return false;
                }
                fieldDescription.Source = nameof(DateTimeParserTool);
                fieldDescription.Description = $"{config?.InputFieldName} parsed as a number";


                this._inputFieldBase = info.GetFieldByName(config.InputFieldName, false);
                if (this._inputFieldBase == null)
                {
                    return false;
                }

                var newRecordInfo = Utilities.CreateRecordInfo(info, fieldDescription);

                this._outputRecordInfo = newRecordInfo;
                this._outputFieldBase = newRecordInfo.GetFieldByName(config.OutputFieldName, false);
                this.Output?.Init(newRecordInfo, nameof(this.Output), null, this.XmlConfig);

                // Create the Copier
                this._copier = Utilities.CreateCopier(info, newRecordInfo, config.OutputFieldName);

                this._culture = CultureTypeConverter.GetCulture(config.Culture);

                return true;
            }

            private bool PushFunc(RecordData r)
            {
                var record = this._outputRecordInfo.CreateRecord();
                this._copier.Copy(record, r);

                string input = this._inputFieldBase.GetAsString(r);

                double value;
                bool result = double.TryParse(input, NumberStyles.Any, this._culture, out value);

                if (result)
                {
                    this._outputFieldBase.SetFromDouble(record, value);
                }
                else
                {
                    this._outputFieldBase.SetNull(record);
                }

                this.Output?.PushRecord(record.GetRecord());
                return true;
            }
        }
    }
}