﻿namespace JDunkerley.AlteryxAddIns
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    using AlteryxRecordInfoNet;

    using Framework;
    using Framework.Attributes;
    using Framework.ConfigWindows;
    using Framework.Factories;
    using Framework.Interfaces;

    public class RollupAggregator :
        BaseTool<RollupAggregator.Config, RollupAggregator.Engine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        public class Config : ConfigWithIncomingConnection
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
                this._inputFieldBase = info.GetFieldByName(this.ConfigObject.SortField, false);
                if (this._inputFieldBase == null)
                {
                    return false;
                }

                this.Output?.Init(FieldDescription.CreateRecordInfo(info));

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
                var culture = CultureTypeConverter.GetCulture(this.ConfigObject.Culture);
                var comparer = StringComparer.Create(culture, this.ConfigObject.IgnoreCase);

                var count = 0;
                foreach (var record in this._data.OrderBy(t => t.Item1, comparer).Select(t => t.Item2))
                {
                    var d = count++ / (double)this._data.Count;
                    this.Output?.UpdateProgress(d, true);
                    this.Output?.Push(record);
                }

                this.Output?.Close(true);
            }
        }
    }
}
