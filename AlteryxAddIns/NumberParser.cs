namespace JDunkerley.AlteryxAddins
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    using AlteryxRecordInfoNet;

    using JDunkerley.AlteryxAddIns.Framework;
    using JDunkerley.AlteryxAddIns.Framework.Attributes;
    using JDunkerley.AlteryxAddIns.Framework.ConfigWindows;

    public class NumberParser :
        BaseTool<NumberParser.Config, NumberParser.Engine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        public class Config
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
            private FieldBase _inputFieldBase;

            private RecordCopier _copier;

            private FieldBase _outputFieldBase;

            public Engine()
            {
                this.Input = new InputProperty(
                    initFunc: this.InitFunc,
                    progressAction: d => this.Output?.UpdateProgress(d, true),
                    pushFunc: this.PushFunc,
                    closedAction: () => this.Output?.Close(true));
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
                this._copier = Utilities.CreateCopier(info, this.Output?.RecordInfo, this.ConfigObject.OutputFieldName);

                return true;
            }

            private bool PushFunc(RecordData r)
            {
                var record = this.Output?.CreateRecord();
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

                this.Output?.Push(record);
                return true;
            }
        }
    }
}