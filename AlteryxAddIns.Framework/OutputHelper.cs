namespace JDunkerley.AlteryxAddIns.Framework
{
    using System;
    using System.Xml;

    using AlteryxRecordInfoNet;

    /// <summary>
    /// Output Helper Class
    /// </summary>
    public class OutputHelper
    {
        private readonly IBaseEngine _hostEngine;

        private readonly string _connectionName;

        private readonly AlteryxRecordInfoNet.PluginOutputConnectionHelper _helper;

        private ulong _recordCount;

        private ulong _recordLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputHelper"/> class.
        /// </summary>
        /// <param name="hostEngine">The host engine.</param>
        /// <param name="connectionName">Name of the outgoing connection.</param>
        public OutputHelper(IBaseEngine hostEngine, string connectionName)
        {
            this._hostEngine = hostEngine;
            this._connectionName = connectionName;

            this._helper = new AlteryxRecordInfoNet.PluginOutputConnectionHelper(this._hostEngine.NToolId, this._hostEngine.Engine);
        }

        /// <summary>
        /// Adds the connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public void AddConnection(AlteryxRecordInfoNet.OutgoingConnection connection)
            => this._helper.AddOutgoingConnection(connection);

        /// <summary>
        ///
        /// </summary>
        /// <param name="recordInfo"></param>
        /// <param name="sortConfig"></param>
        /// <param name="oldConfig"></param>
        public void Init(AlteryxRecordInfoNet.RecordInfo recordInfo, XmlElement sortConfig = null, XmlElement oldConfig = null)
        {
            this.RecordInfo = recordInfo;
            this._lazyRecord = new Lazy<Record>(() => this.CreateRecord());

            this._recordCount = 0;
            this._recordLength = 0;

            this._helper?.Init(recordInfo, this._connectionName, sortConfig, oldConfig ?? this._hostEngine.XmlConfig);
            this._hostEngine.Engine.OutputMessage(this._hostEngine.NToolId, MessageStatus.STATUS_Info, $"Init called back on {this._connectionName}");
        }

        public AlteryxRecordInfoNet.FieldBase this[String fieldName] => this.RecordInfo?.GetFieldByName(fieldName, false);

        /// <summary>
        ///
        /// </summary>
        public AlteryxRecordInfoNet.RecordInfo RecordInfo { get; private set; }

        /// <summary>
        /// Create A New Record
        /// </summary>
        /// <returns></returns>
        public AlteryxRecordInfoNet.Record CreateRecord() => this.RecordInfo?.CreateRecord();

        private Lazy<AlteryxRecordInfoNet.Record> _lazyRecord;

        /// <summary>
        /// Reusable Record
        /// </summary>
        public AlteryxRecordInfoNet.Record Record => this._lazyRecord?.Value;

        public void Push(AlteryxRecordInfoNet.Record record, bool close = false)
        {
            this._helper?.PushRecord(record.GetRecord());

            this._recordCount++;
            this._recordLength += (ulong)((IntPtr)this.RecordInfo.GetRecordLen(record.GetRecord())).ToInt64();

            if (close)
            {
                this.Close();
            }
            else
            {
                this.PushCountAndSize();
            }
        }

        /// <summary>
        /// Update The Progress Of A Connection
        /// </summary>
        /// <param name="percentage">Percentage Progress from 0.0 to 1.0</param>
        /// <param name="setToolProgress">Set Tool Progress As Well</param>
        public void UpdateProgress(double percentage, bool setToolProgress = false)
        {
            this._helper.UpdateProgress(percentage);

            if (setToolProgress)
            {
                this._hostEngine.Engine.OutputToolProgress(this._hostEngine.NToolId, percentage);
            }
        }

        /// <summary>
        /// Tell Alteryx We Are Finished
        /// </summary>
        /// <param name="executionComplete">Tell Alteryx Tool Execution Is Complete</param>
        public void Close(bool executionComplete = false)
        {
            this._helper?.Close();
            this.PushCountAndSize(true);

            if (executionComplete)
            {
                this._hostEngine.ExecutionComplete();
            }

            this.RecordInfo = null;
            this._lazyRecord = null;
        }

        private void PushCountAndSize(bool final = false)
        {
            this._hostEngine.Engine.OutputMessage(
                this._hostEngine.NToolId,
                AlteryxRecordInfoNet.MessageStatus.STATUS_RecordCountAndSize,
                $"{this._connectionName}|{this._recordCount}|{this._recordLength}");
            this._helper.OutputRecordCount(final);
        }
    }
}