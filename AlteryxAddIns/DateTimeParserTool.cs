namespace JDunkerley.AlteryxAddins
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    using AlteryxRecordInfoNet;

    using JDunkerley.AlteryxAddIns.Framework;
    using JDunkerley.AlteryxAddIns.Framework.Attributes;
    using JDunkerley.AlteryxAddIns.Framework.ConfigWindows;

    public class DateTimeParserTool :
        BaseTool<DateTimeParserTool.Config, DateTimeParserTool.Engine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        public class Config
        {
            /// <summary>
            /// Gets or sets the type of the output.
            /// </summary>
            [Category("Output")]
            [Description("Alteryx Type for the Output Field")]
            [TypeConverter(typeof(FixedListTypeConverter<OutputType>))]
            [FieldList(OutputType.Date, OutputType.DateTime, OutputType.String)]
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
        }

        public class Engine : BaseEngine<Config>
        {
            private FieldBase _inputFieldBase;

            private RecordCopier _copier;

            private RecordInfo _outputRecordInfo;

            private FieldBase _outputFieldBase;

            private string _format;

            private bool _exact;

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
                fieldDescription.Description = $"{config?.InputFieldName} parsed as a DateTime";


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

                this._format = config.FormatString;
                this._exact = string.IsNullOrWhiteSpace(this._format);
                this._culture = CultureTypeConverter.GetCulture(config.Culture);

                return true;
            }

            private bool PushFunc(RecordData r)
            {
                var record = this._outputRecordInfo.CreateRecord();
                this._copier.Copy(record, r);

                string input = this._inputFieldBase.GetAsString(r);

                DateTime dt;
                bool result = this._exact
                    ? DateTime.TryParse(input, this._culture, DateTimeStyles.AllowWhiteSpaces, out dt)
                    : DateTime.TryParseExact(input, this._format, this._culture, DateTimeStyles.AllowWhiteSpaces, out dt);

                if (result)
                {
                    this._outputFieldBase.SetFromString(record, dt.ToString("yyyy-MM-dd HH:mm:ss"));
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
