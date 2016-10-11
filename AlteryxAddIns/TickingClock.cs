namespace JDunkerley.AlteryxAddIns
{
    using System;
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;

    using Framework;
    using Framework.Attributes;
    using Framework.ConfigWindows;

    public class TickingClock :
        BaseTool<TickingClock.Config, TickingClock.Engine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        public class Config
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

        public class Engine : BaseEngine<Config>
        {
            private ConcurrentQueue<DateTime> _ticks;

            private AutoResetEvent _resetEvent;

            /// <summary>
            ///
            /// </summary>
            public Engine()
                : base(null)
            {

            }

            /// <summary>
            /// Gets or sets the output.
            /// </summary>
            [CharLabel('O')]
            public OutputHelper Output { get; set; }

            public override bool PI_PushAllRecords(long nRecordLimit)
            {
                if (this.Output == null)
                {
                    this.Engine.OutputMessage(
                        this.NToolId,
                        AlteryxRecordInfoNet.MessageStatus.STATUS_Error,
                        "Output is not set.");
                    return false;
                }

                var fieldDescription = this.ConfigObject.OutputType.OutputDescription(this.ConfigObject.OutputFieldName, 19);
                if (fieldDescription == null)
                {
                    return false;
                }
                fieldDescription.Source = nameof(DateTimeInput);
                fieldDescription.Description = "Ticking Clock";

                var recordInfo = FieldDescription.CreateRecordInfo(fieldDescription);
                this.Output.Init(recordInfo);

                this._ticks = new ConcurrentQueue<DateTime>();
                this._resetEvent = new AutoResetEvent(false);

                if (nRecordLimit == 0)
                {
                    return true;
                }

                var gap = this.ConfigObject.GapBetweenTicks;
                var cancelationToken = new CancellationToken(false);
                Task.Factory.StartNew(
                    () =>
                        {
                            while (!cancelationToken.IsCancellationRequested)
                            {
                                Thread.Sleep(gap);
                                this._ticks?.Enqueue(DateTime.Now);
                                this._resetEvent?.Set();
                            }
                        },
                    cancelationToken,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);

                var recordOut = this.Output.CreateRecord();
                var fmt = this.ConfigObject.OutputType == OutputType.Time ? "HH:mm:ss" : "yyyy-MM-dd HH:mm:ss";

                nRecordLimit = Math.Min(nRecordLimit, this.ConfigObject.RecordLimit);
                while (nRecordLimit > 0)
                {
                    this._resetEvent.WaitOne(250);

                    DateTime tmp;
                    if (this._ticks.TryDequeue(out tmp))
                    {
                        recordOut.Reset();
                        this.Output[this.ConfigObject.OutputFieldName]?.SetFromString(
                            recordOut,
                            tmp.ToString(fmt));
                        this.Output.Push(recordOut, false, 1);
                        nRecordLimit--;
                    }
                }

                this.Output.Close(true);

                this._ticks = null;
                this._resetEvent = null;

                return true;
            }
        }

    }
}
