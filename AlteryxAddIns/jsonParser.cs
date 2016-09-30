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
            /// Gets or sets the name of the input field.
            /// </summary>
            [Description("The Input Field Containing Json to Parse")]
            [TypeConverter(typeof(InputFieldTypeConverter))]
            [InputPropertyName(nameof(Engine.Input), typeof(Engine), FieldType.E_FT_String, FieldType.E_FT_V_String, FieldType.E_FT_V_WString, FieldType.E_FT_WString)]
            public string jsonField { get; set; } = "jsonToParse";

            /// <summary>
            /// ToString used for annotation
            /// </summary>
            /// <returns></returns>
            public override string ToString() => $"{this.jsonField}=>jsonAsCSV";
        }

        public class Engine : BaseEngine<Config>
        {
            private FieldBase _inputFieldBase;

            private FieldBase _outputFieldBase;

            private IRecordCopier _copier;

            private List<string> _data;

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

                var fd = new FieldDescription("jsonAsCSV", FieldType.E_FT_V_WString) {
                    Size = Int16.MaxValue,
                    Source = nameof(jsonParser),
                    Description = $"{this.ConfigObject.jsonField} parsed as CSV"
                };
                this.Output?.Init(FieldDescription.CreateRecordInfo(info, fd));
                this._outputFieldBase = this.Output?["jsonAsCSV"];

                // Create the Copier
                this._copier = this.RecordCopierFactory.CreateCopier(info, this.Output?.RecordInfo);

                this._data = new List<string>();

                return true;
            }

            private bool PushFunc(RecordData r)
            {
                string input = this._inputFieldBase.GetAsString(r);
                this._data.Add(input);
                return true;
            }

            private void ClosedAction()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder("");
                foreach (var record in this._data)
                {
                    sb.Append(record);
                }

                _data.Clear();

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

                //var count = 0;
                foreach (var csvRow in csvRows)
                {
                    var outRecord = this.Output.CreateRecord();
                    if (csvRow != null && csvRow.Trim().Length > 0)
                    {
                        outRecord.Reset();
                        this._outputFieldBase.SetFromString(outRecord, csvRow);

                        this.Output.Push(outRecord);
                    }
                }
                this.Output.Close(true);
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
