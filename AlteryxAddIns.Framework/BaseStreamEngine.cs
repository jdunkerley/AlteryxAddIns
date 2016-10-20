namespace JDunkerley.AlteryxAddIns.Framework
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    using AlteryxRecordInfoNet;

    using Attributes;

    using Interfaces;

    /// <summary>
    /// Base streaming input tool. Has a single output.
    /// </summary>
    /// <typeparam name="TConfig">Configuration object for reading XML into.</typeparam>
    /// <typeparam name="TValue">Type of value queued from background thread to push</typeparam>
    public abstract class BaseStreamEngine<TConfig, TValue> : BaseEngine<TConfig>, IDisposable
        where TConfig : IRecordLimit, new()
    {
        private ConcurrentQueue<TValue> _ticks;

        private ManualResetEventSlim _resetEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseStreamEngine{TConfig, TValue}"/> class.
        /// </summary>
        /// <param name="recordCopierFactory">Factory to create copiers</param>
        /// <param name="outputHelperFactory">Factory to create output helpers</param>
        protected BaseStreamEngine(IRecordCopierFactory recordCopierFactory, IOutputHelperFactory outputHelperFactory)
            : base(recordCopierFactory, outputHelperFactory)
        {
        }

        /// <summary>
        /// Gets or sets the output.
        /// </summary>
        [CharLabel('O')]
        public IOutputHelper Output { get; set; }

        /// <summary>
        /// The PI_PushAllRecords function pointed to by this property will be called by the Alteryx Engine when the plugin should provide all of it's data to the downstream tools.
        /// This is only pertinent to tools which have no upstream (input) connections (such as the Input tool).
        /// </summary>
        /// <param name="nRecordLimit">The nRecordLimit parameter will be &lt; 0 to indicate that there is no limit, 0 to indicate that the tool is being configured and no records should be sent, or &gt; 0 to indicate that only the requested number of records should be sent.</param>
        /// <returns>Return true to indicate you successfully handled the request.</returns>
        public sealed override bool PI_PushAllRecords(long nRecordLimit)
        {
            if (this.Output == null)
            {
                this.Engine.OutputMessage(
                    this.NToolId,
                    AlteryxRecordInfoNet.MessageStatus.STATUS_Error,
                    "Output is not set.");
                return false;
            }

            var recordInfo = this.CreateRecordInfo();
            if (recordInfo == null)
            {
                return false;
            }

            this.Output.Init(recordInfo);

            this._ticks = new ConcurrentQueue<TValue>();
            this._resetEvent = new ManualResetEventSlim(false);

            if (nRecordLimit == 0)
            {
                return true;
            }

            var cancelationToken = new CancellationToken(false);
            Task.Factory.StartNew(
                this.CreateBackgroundAction(cancelationToken),
                cancelationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            var recordOut = this.Output.Record;

            nRecordLimit = Math.Min(nRecordLimit, this.ConfigObject.RecordLimit);
            while (nRecordLimit > 0)
            {
                this._resetEvent.Wait(250);

                TValue tmp;
                if (this._ticks.TryDequeue(out tmp))
                {
                    recordOut.Reset();
                    this.UpdateRecord(recordOut, tmp);
                    this.Output.Push(recordOut, false, 1);
                    nRecordLimit--;
                }
                else
                {
                    this._resetEvent.Reset();
                }
            }

            this.Output.Close(true);

            this._ticks = null;
            this._resetEvent = null;

            return true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Bulk of the disposing method
        /// </summary>
        /// <param name="disposing">Called from Dispose method</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._resetEvent != null)
                {
                    this._resetEvent.Dispose();
                    this._resetEvent = null;
                }
            }
        }

        /// <summary>
        /// Push a record onto the output stream
        /// </summary>
        /// <param name="value">Value to push to the stream. Will be converted to record by <see cref="UpdateRecord"/>.</param>
        protected void PushValue(TValue value)
        {
            this._ticks?.Enqueue(value);
            this._resetEvent?.Set();
        }

        /// <summary>
        /// Create the <see cref="RecordInfo"/> for the output stream
        /// </summary>
        /// <returns>A <see cref="RecordInfo"/> configured with the output columns.</returns>
        protected abstract RecordInfo CreateRecordInfo();

        /// <summary>
        /// Create the background thread that will generate the data stream.
        /// </summary>
        /// <param name="token">Cancellation token which will be set when record limit reached.</param>
        /// <returns>An Action to be run in the background.</returns>
        protected abstract Action CreateBackgroundAction(CancellationToken token);

        /// <summary>
        /// Takes a TValue and loads the data into the Record object
        /// </summary>
        /// <param name="record">A reset Record</param>
        /// <param name="value">Value to read from</param>
        protected abstract void UpdateRecord(Record record, TValue value);
    }
}