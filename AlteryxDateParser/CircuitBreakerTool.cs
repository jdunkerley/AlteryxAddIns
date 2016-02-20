namespace JDunkerley.Alteryx
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Forms.VisualStyles;

    using AlteryxGuiToolkit.Gallery;
    using AlteryxGuiToolkit.Plugins;

    using AlteryxRecordInfoNet;

    using JDunkerley.Alteryx.Attributes;
    using JDunkerley.Alteryx.Framework;

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
        /// Engine for Circuit Breaker
        /// </summary>
        /// <seealso cref="JDunkerley.Alteryx.Framework.BaseEngine{JDunkerley.Alteryx.CircuitBreakerTool.Config}" />
        public class Engine : BaseEngine<Config>
        {
            private Queue<Record> _inputRecords;

            private bool _failed;

            /// <summary>
            /// Initializes a new instance of the <see cref="Engine"/> class.
            /// </summary>
            public Engine()
            {
                this.Breaker = new InputProperty(
                    initFunc: p =>
                        {
                            this._failed = false;
                            return true;
                        },
                    pushFunc: r =>
                        {
                            if (this._failed)
                            {
                                return false;
                            }

                            this._failed = true;
                            this.Output?.UpdateProgress(1);
                            return true;
                        },
                    closedAction: () =>
                        {
                            if (!this._failed)
                            {
                                while ((this._inputRecords?.Count ?? 0) > 0)
                                {
                                    var record = this._inputRecords?.Dequeue();
                                    var recordData = record?.GetRecord();
                                    this.Output?.PushRecord(recordData);
                                }
                            }

                            if (this.Input.State == ConnectionState.Closed)
                            {
                                this.Output?.Close();
                            }
                        });

                this.Input = new InputProperty(
                    initFunc: p =>
                        {
                            this._inputRecords = new Queue<Record>();
                            this.Output?.Init(this.Input.RecordInfo, nameof(this.Output), null, this.XmlConfig);
                            return true;
                        },
                    pushFunc: r =>
                        {
                            if (this._failed)
                            {
                                return false;
                            }
                            
                            if (this.Breaker.State == ConnectionState.Closed)
                            {
                                this.Output?.PushRecord(r);
                            }
                            else
                            {
                                var record = this.Input.RecordInfo.CreateRecord();
                                this.Input.Copier.Copy(record, r);
                                this._inputRecords.Enqueue(record);
                            }

                            return true;
                        },
                    progressAction: p => this.Output?.UpdateProgress(this._failed ? 1.0 : p),
                    closedAction: () =>
                        {
                            if (this.Breaker.State == ConnectionState.Closed)
                            {
                                this.Output?.Close();
                            }
                        });
            }

            [CharLabel('B')]
            [Ordering(1)]
            public InputProperty Breaker { get; } 

            [CharLabel('I')]
            [Ordering(2)]
            public InputProperty Input { get; }

            [CharLabel('O')]
            public PluginOutputConnectionHelper Output { get; set; }
        }
    }
}
