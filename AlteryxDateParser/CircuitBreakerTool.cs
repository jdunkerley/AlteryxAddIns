namespace JDunkerley.Alteryx
{
    using System.Collections.Generic;
    using System.Linq;

    using AlteryxGuiToolkit.Plugins;

    using AlteryxRecordInfoNet;

    using JDunkerley.Alteryx.Attributes;

    public class CircuitBreakerTool
        : BaseTool<CircuitBreakerTool.Config, CircuitBreakerTool.Engine>, IPlugin
    {
        /// <summary>
        /// Configuration Class
        /// </summary>
        public class Config
        {
            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            public override string ToString() => string.Empty;
        }

        /// <summary>
        /// Engine
        /// </summary>
        /// <seealso cref="JDunkerley.Alteryx.BaseEngine{JDunkerley.Alteryx.CircuitBreakerTool.Config}" />
        public class Engine : BaseEngine<Config>
        {
            private List<RecordData> _inputRecords;

            private bool? _failed;

            private bool _finished;

            [CharLabel('B')]
            [Ordering(1)]
            public IncomingConnection Breaker { get; set; }

            [CharLabel('I')]
            [Ordering(2)]
            public IncomingConnection Input { get; set; }

            [CharLabel('O')]
            public PluginOutputConnectionHelper Output { get; set; }

            /// <summary>
            /// Alteryx Initialized An Incoming Connection.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <returns></returns>
            public override bool IncomingConnectionInit(string name)
            {
                switch (name)
                {
                    case nameof(this.Input):
                        this._inputRecords = new List<RecordData>();
                        this._finished = false;
                        if (!this._failed ?? false)
                        {
                            this.Output?.Init(this.Input.RecordInfo, nameof(this.Output), null, this.XmlConfig);
                        }
                        break;
                    case nameof(this.Breaker):
                        this._failed = null;
                        break;
                }

                return true;
            }

            /// <summary>
            /// Called by Alteryx to send each data record to the tool.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="record">The new record</param>
            /// <returns></returns>
            public override bool IncomingConnectionPush(string name, RecordData record)
            {
                switch (name)
                {
                    case nameof(this.Input):
                        if (!this._failed.HasValue)
                        {
                            this._inputRecords.Add(record);
                        } else if (!this._failed.Value)
                        {
                            this.Output?.PushRecord(record);
                        }
                        break;
                    case nameof(this.Breaker):
                        this._failed = true;
                        this._inputRecords = null;
                        this.Output?.UpdateProgress(1.0);
                        this.Output?.Close();
                        return false;
                }

                return true;
            }

            /// <summary>
            /// Called by Alteryx to update progress
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="progress">Progress (0 to 1)</param>
            public override void IncomingConnectionProgress(string name, double progress)
            {
                if (!(this._failed ?? false) && name == nameof(this.Input))
                {
                    this.Output?.UpdateProgress(progress);
                }
            }

            /// <summary>
            /// Alteryx Finished Sending Data For An Incoming Connection.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <returns></returns>
            public override void IncomingConnectionClosed(string name)
            {
                if (this._failed ?? false)
                {
                    return;
                }

                switch (name)
                {
                    case nameof(this.Breaker):
                        foreach (var inputRecord in this._inputRecords ?? Enumerable.Empty<RecordData>())
                        {
                            this.Output?.PushRecord(inputRecord);
                        }
                        this._inputRecords?.Clear();
                        this._failed = false;
                        if (this._finished)
                        {
                            this.Output?.Close();
                        }
                        break;
                    case nameof(this.Input):
                        this._finished = true;
                        if (this._failed.HasValue)
                        {
                            this.Output?.Close();
                        }
                        break;
                }
            }
        }
    }
}
