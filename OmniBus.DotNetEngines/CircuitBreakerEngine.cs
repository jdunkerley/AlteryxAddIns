using System.Collections.Generic;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.EventHandlers;
using OmniBus.Framework.Interfaces;
using OmniBus.Framework.Serialisation;

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
        {
            // Handle Breaker Connection
            this.Breaker = new InputProperty(this);
            this.Breaker.InitCalled += (property, args) => this._failed = false;
            this.Breaker.RecordPushed += this.BreakerOnRecordPushed;
            this.Breaker.Closed += this.BreakerOnClosed;

            // Handle Input Connection
            this.Input = new InputProperty(this);
            this.Input.InitCalled += this.InputOnInitCalled;
            this.Input.RecordPushed += this.InputOnRecordPushed;
            this.Input.ProgressUpdated += (sender, args) => this.Output?.UpdateProgress(
                this._failed ? 1.0 : args,
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

        /// <summary>Create a Serialiser</summary>
        /// <returns><see cref="T:OmniBus.Framework.Serialisation.ISerialiser`1" /> to de-serialise object</returns>
        protected override ISerialiser<CircuitBreakerConfig> Serialiser() => new Serialiser<CircuitBreakerConfig>();

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