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
        public enum OutputType
        {
            Date,
            DateTime,
            String
        }

        public class Config
        {
            /// <summary>
            /// Gets or sets the type of the output.
            /// </summary>
            public OutputType OutputType { get; set; }

            /// <summary>
            /// Gets or sets the name of the output field.
            /// </summary>
            public string OutputFieldName { get; set; } = "Date";

            /// <summary>
            /// Gets or sets the culture.
            /// </summary>
            public string Culture { get; set; } = CultureTypeConverter.Current;

            /// <summary>
            /// Gets or sets the name of the input field.
            /// </summary>
            [TypeConverter(typeof(InputFieldTypeConverter))]
            [InputPropertyName(nameof(Engine.Input), typeof(Engine), FieldType.E_FT_String, FieldType.E_FT_V_String, FieldType.E_FT_V_WString, FieldType.E_FT_WString)]
            public string InputFieldName { get; set; } = "DateInput";

            /// <summary>
            /// Gets or sets the input format.
            /// </summary>
            public string InputFormat { get; set; }

            /// <summary>
            /// ToString used for annotation
            /// </summary>
            /// <returns></returns>
            public override string ToString() => $"{this.InputFieldName}=>{this.OutputFieldName}";

            /// <summary>
            /// Create a FieldDescription Object
            /// </summary>
            /// <returns></returns>
            public FieldDescription OutputDescription()
            {
                switch (this.OutputType)
                {
                    case OutputType.Date:
                        return new FieldDescription(this.OutputFieldName, FieldType.E_FT_Date);
                    case OutputType.DateTime:
                        return new FieldDescription(this.OutputFieldName, FieldType.E_FT_DateTime);
                    case OutputType.String:
                        return new FieldDescription(this.OutputFieldName, FieldType.E_FT_String)
                                   {
                                       Size = 19,
                                       Source = nameof(DateTimeParserTool),
                                       Description = $"{this.InputFieldName} parsed as a DateTime"
                                   };
                }

                return null;
            }

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

                var fieldDescription = config?.OutputDescription();
                if (fieldDescription == null)
                {
                    return false;
                }

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

                this._format = config.InputFormat;
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
