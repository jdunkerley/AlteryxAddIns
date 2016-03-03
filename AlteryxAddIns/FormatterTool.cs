namespace JDunkerley.AlteryxAddins
{
    using System;
    using System.ComponentModel;

    using AlteryxRecordInfoNet;

    using JDunkerley.AlteryxAddIns.Framework;
    using JDunkerley.AlteryxAddIns.Framework.Attributes;
    using JDunkerley.AlteryxAddIns.Framework.ConfigWindows;

    public class FormatterTool :
        BaseTool<FormatterTool.Config, FormatterTool.Engine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        public class Config
        {
            /// <summary>
            /// Gets or sets the name of the output field.
            /// </summary>
            public string OutputFieldName { get; set; } = "Date";

            /// <summary>
            /// Gets or sets the length of the output field.
            /// </summary>
            public int OutputFieldLength { get; set; } = 64;

            /// <summary>
            /// Gets or sets the culture.
            /// </summary>
            [TypeConverter(typeof(CultureTypeConverter))]
            public string Culture { get; set; } = CultureTypeConverter.Current;

            /// <summary>
            /// Gets or sets the name of the input field.
            /// </summary>
            [TypeConverter(typeof(InputFieldTypeConverter))]
            [InputPropertyName(nameof(Engine.Input), typeof(Engine), FieldType.E_FT_Bool, FieldType.E_FT_Byte, FieldType.E_FT_Int16, FieldType.E_FT_Int32, FieldType.E_FT_Int64, FieldType.E_FT_Float, FieldType.E_FT_Double, FieldType.E_FT_FixedDecimal, FieldType.E_FT_Date, FieldType.E_FT_DateTime, FieldType.E_FT_Time)]
            public string InputFieldName { get; set; } = "DateInput";

            /// <summary>
            /// Gets or sets the input format.
            /// </summary>
            public string OutputFormat { get; set; }

            /// <summary>
            /// ToString used for annotation
            /// </summary>
            /// <returns></returns>
            public override string ToString() => $"{this.InputFieldName}=>{this.OutputFieldName}";
        }

        public class Engine : BaseEngine<Config>
        {
            private RecordCopier _copier;

            private RecordInfo _outputRecordInfo;

            private FieldBase _outputFieldBase;

            private Func<RecordData, string> _formatter;

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

                var fieldDescription = new FieldDescription(config.OutputFieldName, FieldType.E_FT_V_WString) { Size = config.OutputFieldLength };

                var inputFieldBase = info.GetFieldByName(config.InputFieldName, false);
                if (inputFieldBase == null)
                {
                    return false;
                }

                var newRecordInfo = Utilities.CreateRecordInfo(info, fieldDescription);

                this._outputRecordInfo = newRecordInfo;
                this._outputFieldBase = newRecordInfo.GetFieldByName(config.OutputFieldName, false);
                this.Output?.Init(newRecordInfo, nameof(this.Output), null, this.XmlConfig);

                // Create the Copier
                this._copier = Utilities.CreateCopier(info, newRecordInfo, config.OutputFieldName);

                var format = config.OutputFormat;
                var culture = CultureTypeConverter.GetCulture(config.Culture);

                if (string.IsNullOrWhiteSpace(format))
                {
                    this._formatter = r => inputFieldBase.GetAsString(r);
                }
                else
                {
                    switch (inputFieldBase.FieldType)
                    {
                        case FieldType.E_FT_Bool:
                            this._formatter = r => inputFieldBase.GetAsBool(r)?.ToString(culture);
                            break;
                        case FieldType.E_FT_Byte:
                        case FieldType.E_FT_Int16:
                        case FieldType.E_FT_Int32:
                        case FieldType.E_FT_Int64:
                            this._formatter = r => inputFieldBase.GetAsInt64(r)?.ToString(format, culture);
                            break;
                        case FieldType.E_FT_Float:
                        case FieldType.E_FT_Double:
                        case FieldType.E_FT_FixedDecimal:
                            this._formatter = r => inputFieldBase.GetAsDouble(r)?.ToString(format, culture);
                            break;
                        case FieldType.E_FT_Date:
                        case FieldType.E_FT_DateTime:
                            this._formatter = r => inputFieldBase.GetAsString(r).ToDateTime()?.ToString(format, culture);
                            break;
                        case FieldType.E_FT_Time:
                            this._formatter = r => inputFieldBase.GetAsString(r).ToTimeSpan()?.ToString(format, culture);
                            break;
                    }
                }

                return true;
            }

            private bool PushFunc(RecordData r)
            {
                var record = this._outputRecordInfo.CreateRecord();
                this._copier.Copy(record, r);

                string result = this._formatter(r);

                if (result != null)
                {
                    this._outputFieldBase.SetFromString(record, result);
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