namespace JDunkerley.AlteryxAddIns
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    using Newtonsoft.Json.Linq;

    using AlteryxRecordInfoNet;

    using Framework;
    using Framework.Attributes;
    using Framework.ConfigWindows;
    using Framework.Factories;
    using Framework.Interfaces;

    public class jsonParser :
        BaseTool<jsonParser.Config, jsonParser.Engine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        public class Config : ConfigWithIncomingConnection
        {
            /// <summary>
            /// Specify the name of the field for the X co-ordinate
            /// </summary>
            [Category("Output")]
            [Description("Json Output as CSV String - from here, split to columns using Parse Tool")]
            public string OutputParsedJson { get; set; } = "jsonOutCSV";

            /// <summary>
            /// Gets or sets the name of the input field.
            /// </summary>
            [Description("The Input Field Containing Json to Parse")]
            [TypeConverter(typeof(InputFieldTypeConverter))]
            [InputPropertyName(nameof(Engine.Input), typeof(Engine), FieldType.E_FT_String, FieldType.E_FT_V_String, FieldType.E_FT_V_WString, FieldType.E_FT_WString)]
            public string jsonField { get; set; } = "jsonToParse";
        }

        public class Engine : BaseEngine<Config>
        {
            private FieldBase _inputFieldBase;

            private FieldBase _outputFieldBase;

            private IRecordCopier _copier;

            private List<Tuple<string, Record>> _data;

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
                this.Input.RecordPushed += (sender, args) => args.Success = this.PushFunc(args.RecordData);
                this.Input.Closed += (sender, args) => this.ClosedAction();
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
                this._inputFieldBase = info.GetFieldByName(this.ConfigObject.jsonField, false);
                if (this._inputFieldBase == null)
                {
                    return false;
                }

                this.Output?.Init(FieldDescription.CreateRecordInfo(info));

                this._outputFieldBase = this.Output?[this.ConfigObject.OutputParsedJson];

                // Create the Copier
                this._copier = this.RecordCopierFactory.CreateCopier(info, this.Output?.RecordInfo);

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
                System.Text.StringBuilder sb = new System.Text.StringBuilder("");
                foreach (var record in this._data)
                {
                    sb.Append(record.Item1);
                }

                var obj = JObject.Parse(sb.ToString());

                // Collect column titles: all property names whose values are of type JValue, distinct, in order of encountering them.
                var values = obj.DescendantsAndSelf()
                    .OfType<JProperty>()
                    .Where(p => p.Value is JValue)
                    .GroupBy(p => p.Name)
                    .ToList();

                var columns = values.Select(g => g.Key).ToArray();

                // Filter JObjects that have child objects that have values.
                var parentsWithChildren = values.SelectMany(g => g).SelectMany(v => v.AncestorsAndSelf().OfType<JObject>().Skip(1)).ToHashSet();

                // Collect all data rows: for every object, go through the column titles and get the value of that property in the closest ancestor or self that has a value of that name.
                var rows = obj
                    .DescendantsAndSelf()
                    .OfType<JObject>()
                    .Where(o => o.PropertyValues().OfType<JValue>().Any())
                    .Where(o => o == obj || !parentsWithChildren.Contains(o)) // Show a row for the root object + objects that have no children.
                    .Select(o => columns.Select(c => o.AncestorsAndSelf()
                        .OfType<JObject>()
                        .Select(parent => parent[c])
                        .Where(v => v is JValue)
                        .Select(v => (string)v)
                        .FirstOrDefault())
                        .Reverse() // Trim trailing nulls
                        .SkipWhile(s => s == null)
                        .Reverse());

                var csvRows = new[] { columns }.Concat(rows).Select(r => string.Join(",", r));

                var count = 0;
                foreach (var csvRow in csvRows)
                {
                    var d = count++ / (double)this._data.Count;
                    this.Output?.UpdateProgress(d, true);

                    var record = this.Output.CreateRecord();
                    this._outputFieldBase.SetFromString(record, csvRow);
                    this.Output?.Push(record);
                }
                this.Output?.Close(true);
            }
        }
    }

    public static class EnumerableExtensions
    {
        // http://stackoverflow.com/questions/3471899/how-to-convert-linq-results-to-hashset-or-hashedset
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }
    }
}
