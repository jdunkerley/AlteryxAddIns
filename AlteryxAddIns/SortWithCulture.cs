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
            public OutputHelper Output { get; set; }

            private bool InitFunc(RecordInfo info)
            {
                this._inputFieldBase = info.GetFieldByName(this.ConfigObject.SortField, false);
                if (this._inputFieldBase == null)
                {
                    return false;
                }

                this.Output?.Init(Utilities.CreateRecordInfo(info));

                // Create the Copier
                this._copier = Utilities.CreateCopier(info, this.Output?.RecordInfo);

                this._data = new List<Tuple<string, Record>>();

                return true;
            }

            private bool PushFunc(RecordData r)
            {
                var record = this.Output.CreateRecord();
                this._copier.Copy(record, r);

                string input = this._inputFieldBase.GetAsString(r);
                this._data.Add(Tuple.Create(input, record));
                return true;
            }

            private void ClosedAction()
            {
                var culture = CultureTypeConverter.GetCulture(this.ConfigObject.Culture);
                var comparer = StringComparer.Create(culture, this.ConfigObject.IgnoreCase);

                int count = 0;
                foreach (var record in this._data.OrderBy(t=>t.Item1, comparer).Select(t => t.Item2))
                {
                    double d = count++ / (double)this._data.Count;
                    this.Output?.UpdateProgress(d, true);
                    this.Output?.Push(record);
                }

                this.Output?.Close(true);
            }
        }
    }
}
