namespace JDunkerley.Alteryx
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    using AlteryxGuiToolkit.Plugins;

    using AlteryxRecordInfoNet;

    using JDunkerley.Alteryx.Attributes;
    using JDunkerley.Alteryx.Framework;

    public class DateTimeParserTool :
        BaseTool<DateTimeParserTool.Config, DateTimeParserTool.Engine>, IPlugin

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
            /// Gets or sets the name of the input field.
            /// </summary>
            [TypeConverter(typeof(InputFieldTypeConverter))]
            [InputPropertyName(nameof(Engine.Input), typeof(Engine))]
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
        }

        public class Engine : BaseEngine<Config>
        {
            private FieldBase _inputFieldBase;

            private RecordCopier _copier;

            private RecordInfo _outputRecordInfo;

            private FieldBase _outputFieldBase;

            public Engine()
            {
                this.Input = new InputProperty(
                    initFunc: this.InitFunc,
                    progressAction: d => this.Output.UpdateProgress(d),
                    pushFunc: this.PushFunc);
            }

            private bool InitFunc(RecordInfo info)
            {
                var config = this.GetConfigObject();

                this._inputFieldBase = info.GetFieldByName(config.InputFieldName, true);

                var newRecordInfo = new RecordInfo();

                bool added = false;
                for (int i = 0; i < info.NumFields(); i++)
                {
                    var fieldInfo = info[i];
                    if (fieldInfo.GetFieldName() == config.OutputFieldName)
                    {
                        this.AddOutputField(config, newRecordInfo);
                        added = true;
                        continue;
                    }
                    newRecordInfo.AddField(fieldInfo);
                }

                if (!added)
                {
                    this.AddOutputField(config, newRecordInfo);
                }

                this._outputRecordInfo = newRecordInfo;
                this._outputFieldBase = newRecordInfo.GetFieldByName(config.OutputFieldName, false);
                this.Output?.Init(newRecordInfo, nameof(this.Output), null, this.XmlConfig);

                // Create the Copier
                this._copier = Utilities.CreateCopier(info, newRecordInfo, config.OutputFieldName);

                return true;
            }

            private void AddOutputField(Config config, RecordInfo newRecordInfo)
            {
                switch (config.OutputType)
                {
                    case OutputType.Date:
                        newRecordInfo.AddField(config.OutputFieldName, FieldType.E_FT_Date);
                        break;
                    case OutputType.DateTime:
                        newRecordInfo.AddField(config.OutputFieldName, FieldType.E_FT_DateTime);
                        break;
                    case OutputType.String:
                        newRecordInfo.AddField(
                            config.OutputFieldName,
                            FieldType.E_FT_String,
                            19,
                            1,
                            "DateTimeParserTool",
                            "Parsed Date Time Field");
                        break;
                }
            }

            private bool PushFunc(RecordData r)
            {
                var record = this._outputRecordInfo.CreateRecord();
                this._copier.Copy(record, r);

                string input = this._inputFieldBase.GetAsString(r);


                DateTime dt;
                var inputFormat = this.GetConfigObject().InputFormat;
                if (string.IsNullOrWhiteSpace(inputFormat))
                {
                    if (DateTime.TryParse(
                        input,
                        CultureInfo.CurrentCulture,
                        DateTimeStyles.AllowWhiteSpaces,
                        out dt))
                    {
                        this._outputFieldBase.SetFromString(record, dt.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    else
                    {
                        this._outputFieldBase.SetNull(record);
                    }
                }
                else
                {
                    if (DateTime.TryParseExact(
                        input,
                        inputFormat,
                        CultureInfo.CurrentCulture,
                        DateTimeStyles.AllowWhiteSpaces,
                        out dt))
                    {
                        this._outputFieldBase.SetFromString(record, dt.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    else
                    {
                        this._outputFieldBase.SetNull(record);
                    }
                }



                this.Output?.PushRecord(record.GetRecord());
                return true;
            }

            [CharLabel('I')]
            public InputProperty Input { get; }

            /// <summary>
            /// Gets or sets the output.
            /// </summary>
            [CharLabel('O')]
            public PluginOutputConnectionHelper Output { get; set; }
        }
    }
}
