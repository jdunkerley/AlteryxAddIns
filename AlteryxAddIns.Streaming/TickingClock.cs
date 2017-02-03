namespace JDunkerley.AlteryxAddIns.Streaming
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;

    using Framework;
    using Framework.Attributes;
    using Framework.ConfigWindows;
    using Framework.Interfaces;

    using Framework.Factories;

    public class TickingClock : BaseTool<TickingClock.Config, TickingClock.Engine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        public class Config : IRecordLimit
        {
            private TimeSpan _gapBetweenTicks;

            /// <summary>
            /// Gets or sets the type of the output.
            /// </summary>
            [Category("Output")]
            [Description("Alteryx Type for the Output Field")]
            [TypeConverter(typeof(FixedListTypeConverter<OutputType>))]
            [FieldList(OutputType.Date, OutputType.DateTime, OutputType.Time, OutputType.String)]
            public OutputType OutputType { get; set; } = OutputType.DateTime;

            /// <summary>
            /// Gets or sets the name of the output field.
            /// </summary>
            [Category("Output")]
            [Description("Field Name To Use For Output Field")]
            public string OutputFieldName { get; set; } = "Date";

            /// <summary>
            /// Gets or sets the time between updates.
            /// </summary>
            [Description("Time between ticks (minimum 1 second)")]
            public TimeSpan GapBetweenTicks
            {
                get
                {
                    return this._gapBetweenTicks;
                }
                set
                {
                    if (value < TimeSpan.FromSeconds(1))
                    {
                        value = TimeSpan.FromSeconds(1);
                    }

                    this._gapBetweenTicks = value;
                }
            }

            /// <summary>
            /// Run until this many ticks.
            /// If set to 0 run indefinitely.
            /// </summary>
            [Category("Output")]
            [Description("Limit to this many record, or 0 for run indefinitely.")]
            public long RecordLimit { get; set; }
        }

        public class Engine : BaseStreamEngine<Config, DateTime>
        {
            private string _fmt;

            /// <summary>
            /// Constructor for Alteryx Engine
            /// </summary>
            public Engine()
                : this(new OutputHelperFactory())
            {
            }

            /// <summary>
            /// Create An Engine for unit testing.
            /// </summary>
            /// <param name="outputHelperFactory">Factory to create output helpers</param>
            internal Engine(IOutputHelperFactory outputHelperFactory)
                : base(null, outputHelperFactory)
            {
            }

            /// <summary>
            /// Called after <see cref="BaseEngine{TConfig}.PI_Init"/> is done.
            /// </summary>
            protected override void OnInitCalled()
            {
                this._fmt = this.ConfigObject.OutputType == OutputType.Time ? "HH:mm:ss" : "yyyy-MM-dd HH:mm:ss";
            }

            protected override IEnumerable<FieldDescription> CreateFieldDescriptions()
            {
                var fieldDescription = this.ConfigObject.OutputType.OutputDescription(
                    this.ConfigObject.OutputFieldName,
                    source: nameof(TickingClock),
                    description: "Time on the local machine");

                if (fieldDescription == null)
                {
                    return Enumerable.Empty<FieldDescription>();
                }

                return new[] { fieldDescription };
            }

            protected override Action CreateBackgroundAction(CancellationToken token)
            {
                var gap = this.ConfigObject.GapBetweenTicks;

                return () =>
                    {
                        while (!token.IsCancellationRequested)
                        {
                            Thread.Sleep(gap);
                            this.PushValue(DateTime.Now);
                        }
                    };
            }

            protected override void UpdateRecord(AlteryxRecordInfoNet.Record record, DateTime value)
            {
                this.Output[this.ConfigObject.OutputFieldName]?.SetFromString(record, value.ToString(this._fmt));
            }
        }
    }
}