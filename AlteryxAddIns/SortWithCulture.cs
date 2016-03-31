namespace JDunkerley.AlteryxAddins
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    using AlteryxRecordInfoNet;

    using JDunkerley.AlteryxAddIns.Framework;
    using JDunkerley.AlteryxAddIns.Framework.Attributes;
    using JDunkerley.AlteryxAddIns.Framework.ConfigWindows;

    public class SortWithCulture :
        BaseTool<SortWithCulture.Config, SortWithCulture.Engine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        public class Config
        {
            /// <summary>
            /// Gets or sets the culture.
            /// </summary>
            [TypeConverter(typeof(CultureTypeConverter))]
            [Description("The Culture Used To Sort The Value")]
            public string Culture { get; set; } = CultureTypeConverter.Current;

            /// <summary>
            /// Gets or sets the flag to sort with case.
            /// </summary>
            [Description("Sort Ignoring Case")]
            public bool IgnoreCase { get; set; } = false;

            /// <summary>
            /// Gets or sets the name of the input field.
            /// </summary>
            [Description("The Field On Input Stream To Sort On")]
            [TypeConverter(typeof(InputFieldTypeConverter))]
            [InputPropertyName(nameof(Engine.Input), typeof(Engine), FieldType.E_FT_String, FieldType.E_FT_V_String, FieldType.E_FT_V_WString, FieldType.E_FT_WString)]
            public string SortField { get; set; } = "ToSort";


            /// <summary>
            /// ToString used for annotation
            /// </summary>
            /// <returns></returns>
            public override string ToString() => $"Ordered by {this.SortField}";
        }

        public class Engine : BaseEngine<Config>
        {
            private FieldBase _inputFieldBase;

            private RecordCopier _copier;

            private List<Tuple<string, Record>> _data;

            private RecordInfo _outputRecordInfo;

            private ulong _recordCount;
            private ulong _recordLength;

            public Engine()
            {
                this.Input = new InputProperty(
                    initFunc: this.InitFunc,
                    pushFunc: this.PushFunc,
                    closedAction: this.ClosedAction);
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
                this._inputFieldBase = info.GetFieldByName(this.ConfigObject.SortField, false);
                if (this._inputFieldBase == null)
                {
                    return false;
                }

                this._outputRecordInfo = Utilities.CreateRecordInfo(info);
                this.Output?.Init(this._outputRecordInfo, nameof(this.Output), null, this.XmlConfig);

                // Create the Copier
                this._copier = Utilities.CreateCopier(info, this._outputRecordInfo);

                this._data = new List<Tuple<string, Record>>();

                this._recordCount = 0;
                this._recordLength = 0;

                return true;
            }

            private bool PushFunc(RecordData r)
            {
                var record = this._outputRecordInfo.CreateRecord();
                this._copier.Copy(record, r);

                string input = this._inputFieldBase.GetAsString(r);
                this._data.Add(Tuple.Create(input, record));
                return true;
            }

            private void ClosedAction()
            {
                var culture = CultureTypeConverter.GetCulture(this.ConfigObject.Culture);
                var comparer = StringComparer.Create(culture, this.ConfigObject.IgnoreCase);

                foreach (var record in this._data.OrderBy(t=>t.Item1, comparer).Select(t => t.Item2))
                {
                    var recordData = record.GetRecord();
                    this._recordCount++;
                    this._recordLength += (ulong)((IntPtr)this._outputRecordInfo.GetRecordLen(recordData)).ToInt64();

                    double d = this._recordCount / (double)this._data.Count;
                    this.Output.UpdateProgress(d);
                    this.Engine.OutputToolProgress(this.NToolId, d);

                    this.Output?.PushRecord(record.GetRecord());
                    this.Engine.OutputMessage(
                        this.NToolId,
                        MessageStatus.STATUS_RecordCountAndSize,
                        $"{this._recordCount}\n{this._recordLength}");
                }

                this.Output?.Close();
                this.ExecutionComplete();
            }
        }
    }
}
