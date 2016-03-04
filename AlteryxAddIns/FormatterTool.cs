namespace JDunkerley.AlteryxAddins
{
    using System;
    using System.ComponentModel;

    using AlteryxRecordInfoNet;

    using JDunkerley.AlteryxAddIns.Framework;
    using JDunkerley.AlteryxAddIns.Framework.Attributes;
    using JDunkerley.AlteryxAddIns.Framework.ConfigWindows;

    /// <summary>
    /// Take a value and format as a string
    /// </summary>
    public class FormatterTool :
        BaseTool<FormatterTool.Config, FormatterTool.Engine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        /// <summary>
        /// Configuration object for the Formatter Tool
        /// </summary>
        public class Config
        {
            /// <summary>
            /// Specify the name of the  formatted field in the Output
            /// </summary>
            [Category("Output")]
            public string OutputFieldName { get; set; } = "Formatted";

            /// <summary>
            /// Specify the length of the Output field
            /// </summary>
            [Category("Output")]
            public int OutputFieldLength { get; set; } = 64;

            /// <summary>
            /// Specify the culture to use for formatting the value
            /// </summary>
            [Category("Format")]
            [TypeConverter(typeof(CultureTypeConverter))]
            public string Culture { get; set; } = CultureTypeConverter.Current;

            /// <summary>
            /// Specify the name of the field to format
            /// </summary>
            [Category("Input")]
            [TypeConverter(typeof(InputFieldTypeConverter))]
            [InputPropertyName(nameof(Engine.Input), typeof(Engine), FieldType.E_FT_Bool, FieldType.E_FT_Byte, FieldType.E_FT_Int16, FieldType.E_FT_Int32, FieldType.E_FT_Int64, FieldType.E_FT_Float, FieldType.E_FT_Double, FieldType.E_FT_FixedDecimal, FieldType.E_FT_Date, FieldType.E_FT_DateTime, FieldType.E_FT_Time)]
            public string InputFieldName { get; set; } = "DateInput";

            /// <summary>
            /// Specify the format to be applied
            /// </summary>
            [Category("Format")]
            public string FormatString { get; set; }

            /// <summary>
            /// ToString used for annotation
            /// </summary>
            /// <returns></returns>
            public override string ToString() => $"{this.InputFieldName}=>{this.OutputFieldName} [{this.FormatString}]";
        }

        /// <summary>
        /// Formatter Tool Engine
        /// </summary>
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

                // Get Input Field
                var inputFieldBase = info.GetFieldByName(config.InputFieldName, false);
                if (inputFieldBase == null)
                {
                    return false;
                }

                // Create Output Format
                var fieldDescription = new FieldDescription(config.OutputFieldName, FieldType.E_FT_V_WString) { Size = config.OutputFieldLength };
                this._outputRecordInfo = Utilities.CreateRecordInfo(info, fieldDescription);
                this._outputFieldBase = this._outputRecordInfo.GetFieldByName(config.OutputFieldName, false);
                this.Output?.Init(this._outputRecordInfo, nameof(this.Output), null, this.XmlConfig);

                // Create the Copier
                this._copier = Utilities.CreateCopier(info, this._outputRecordInfo, config.OutputFieldName);

                // Create the Formatter funcxtion
                this._formatter = this.CreateFormatter(config, inputFieldBase);

                return this._formatter != null;
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

            private Func<RecordData, string> CreateFormatter(Config config, FieldBase inputFieldBase)
            {
                var format = config.FormatString;
                var culture = CultureTypeConverter.GetCulture(config.Culture);

                if (string.IsNullOrWhiteSpace(format))
                {
                    return inputFieldBase.GetAsString;
                }
                else
                {
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
                            return r => inputFieldBase.GetAsString(r).ToDateTime()?.ToString(format, culture);
                        case FieldType.E_FT_Time:
                            return r => inputFieldBase.GetAsString(r).ToTimeSpan()?.ToString(format, culture);
                    }
                }

                return null;
            }
        }
    }
}