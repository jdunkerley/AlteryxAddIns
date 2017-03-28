using System.Collections.Generic;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.EventHandlers;
using OmniBus.Framework.Factories;
using OmniBus.Framework.Interfaces;

namespace OmniBus
{
    /// <summary>
    ///     Engine Class Acting As A Circuit Breaker
    ///     If Data Passed To Breaker Input Then No Data Will Be Output
    ///     Otherwise Input Will Be Passed Through
    /// </summary>
    /// <seealso cref="BaseEngine{TConfig}" />
    public class CircuitBreakerEngine : BaseEngine<CircuitBreakerConfig>
    {
        private bool _failed;

        private Queue<Record> _inputRecords;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CircuitBreakerEngine" /> class.
        ///     Constructor For Alteryx
        /// </summary>
        public CircuitBreakerEngine()
            : this(new RecordCopierFactory(), new InputPropertyFactory(), new OutputHelperFactory())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CircuitBreakerEngine" /> class.
        ///     Create An Engine for unit testing.
        /// </summary>
        /// <param name="recordCopierFactory">Factory to create copiers</param>
        /// <param name="inputPropertyFactory">Factory to create input properties</param>
        /// <param name="outputHelperFactory">Factory to create output helpers</param>
        internal CircuitBreakerEngine(
            IRecordCopierFactory recordCopierFactory,
            IInputPropertyFactory inputPropertyFactory,
            IOutputHelperFactory outputHelperFactory)
            : base(recordCopierFactory, outputHelperFactory)
        {
            // Handle Breaker Connection
            this.Breaker = inputPropertyFactory.Build(recordCopierFactory, this.ShowDebugMessages);
            this.Breaker.InitCalled += (property, args) => this._failed = false;
            this.Breaker.RecordPushed += this.BreakerOnRecordPushed;
            this.Breaker.Closed += this.BreakerOnClosed;

            // Handle Input Connection
            this.Input = inputPropertyFactory.Build(recordCopierFactory, this.ShowDebugMessages);
            this.Input.InitCalled += this.InputOnInitCalled;
            this.Input.RecordPushed += this.InputOnRecordPushed;
            this.Input.ProgressUpdated += (sender, args) => this.Output?.UpdateProgress(
                this._failed ? 1.0 : args.Progress,
                true);
            this.Input.Closed += this.InputOnClosed;
        }

        /// <summary>
        ///     Gets the input connection for the breaker - If Any Rows Then Block The Input
        /// </summary>
        [CharLabel('B')]
        [Ordering(1)]
        public IInputProperty Breaker { get; }

        /// <summary>
        ///     Gets the input connection for data - Passed Through If No Rows On Breaker
        /// </summary>
        [CharLabel('I')]
        [Ordering(2)]
        public IInputProperty Input { get; }

        /// <summary>
        ///     Gets or sets the Data output connection
        /// </summary>
        [CharLabel('O')]
        public IOutputHelper Output { get; set; }

        private void BreakerOnRecordPushed(IInputProperty sender, RecordPushedEventArgs args)
        {
            if (this._failed)
            {
                args.Success = false;
                return;
            }

            this._failed = true;
            this.ExecutionComplete();
        }

        private void BreakerOnClosed(IInputProperty sender)
        {
            if (!this._failed)
            {
                while ((this._inputRecords?.Count ?? 0) > 0)
                {
                    var record = this._inputRecords?.Dequeue();
                    this.Output?.Push(record);
                }
            }

            if (this.Input.State == ConnectionState.Closed)
            {
                this.Output?.Close(true);
            }
        }

        private void InputOnInitCalled(IInputProperty property, SuccessEventArgs args)
        {
            this._inputRecords = new Queue<Record>();
            this.Output?.Init(this.Input.RecordInfo);
        }

        private void InputOnRecordPushed(IInputProperty sender, RecordPushedEventArgs args)
        {
            if (this._failed)
            {
                args.Success = false;
                return;
            }

            var record = this.Input.RecordInfo.CreateRecord();
            this.Input.Copier.Copy(record, args.RecordData);

            if (this.Breaker.State == ConnectionState.Closed)
            {
                this.Output?.Push(record);
            }
            else
            {
                this._inputRecords.Enqueue(record);
            }
        }

        private void InputOnClosed(IInputProperty sender)
        {
            if (this.Breaker.State == ConnectionState.Closed)
            {
                this.Output?.Close(true);
            }
        }
    }
}