using System;
using System.Xml;
using System.Xml.Serialization;
using AlteryxRecordInfoNet;

namespace JDunkerley.Alteryx
{

    public class DateTimeParserEngine : INetPlugin
    {
        public class Config
        {
            /// <summary>
            /// Return A DateTime Instead Of A Date
            /// </summary>
            public bool ReturnDateTime { get; set; }

            /// <summary>
            /// Field Name For Output
            /// </summary>
            public string OutputFieldName { get; set; } = "Date";
        }

        private PluginOutputConnectionHelper OutputHelper { get; set; }
        private int NToolId { get; set; }
        private EngineInterface Engine { get; set; }
        private XmlElement XmlConfig { get; set; }
        private Config ConfigObject { get; set; }

        public DateTimeParserEngine()
        {
            System.Diagnostics.Debug.WriteLine("Engine Built");
        }

        /// <summary>
        /// Called by Alteryx to Initialize The Tool
        /// </summary>
        /// <param name="nToolId">Tool ID</param>
        /// <param name="engineInterface">Connection to Alteryx Engine (for logging etc)</param>
        /// <param name="pXmlProperties">Configuration details</param>
        public void PI_Init(int nToolId, EngineInterface engineInterface, XmlElement pXmlProperties)
        {
            this.NToolId = nToolId;
            this.Engine = engineInterface;

            this.XmlConfig = pXmlProperties;

            this.OutputHelper = new PluginOutputConnectionHelper(this.NToolId, this.Engine);

            this.Engine?.OutputMessage(this.NToolId, MessageStatus.STATUS_Info, "PI_Init Called");
        }

        /// <summary>
        /// Handle Incoming Connections Being Added
        /// </summary>
        /// <param name="pIncomingConnectionType"></param>
        /// <param name="pIncomingConnectionName"></param>
        /// <returns></returns>
        public IIncomingConnectionInterface PI_AddIncomingConnection(string pIncomingConnectionType,
            string pIncomingConnectionName)
        {
            throw new NotImplementedException("No incoming connections on this pass");
        }

        /// <summary>
        /// Handle Outgoing Connections Being Added
        /// </summary>
        /// <param name="pOutgoingConnectionName"></param>
        /// <param name="outgoingConnection"></param>
        /// <returns></returns>
        public bool PI_AddOutgoingConnection(string pOutgoingConnectionName, OutgoingConnection outgoingConnection)
        {
            var helper = this.OutputHelper;
            if (helper == null)
            {
                return false;
            }

            helper.AddOutgoingConnection(outgoingConnection);
            return true;
        }

        /// <summary>
        /// Called only if you have no Input Connections
        /// </summary>
        /// <param name="nRecordLimit"></param>
        /// <returns></returns>
        public bool PI_PushAllRecords(long nRecordLimit)
        {
            var recordInfo = new RecordInfo();
            recordInfo.AddField("Date", FieldType.E_FT_Date);

            this.OutputHelper?.Init(recordInfo, "Output", null, this.XmlConfig);
            if (nRecordLimit == 0)
            {
                this.Engine?.OutputMessage(this.NToolId, MessageStatus.STATUS_Complete, "");
                this.OutputHelper?.Close();
                return true;
            }

            var recordOut = recordInfo.CreateRecord();
            recordInfo.GetFieldByName("Date", false)?.SetFromString(recordOut, DateTime.Today.ToString("yyyy-MM-dd"));
            this.OutputHelper?.PushRecord(recordOut.GetRecord());
            this.OutputHelper?.UpdateProgress(1.0);
            this.OutputHelper?.OutputRecordCount(true);

            this.Engine?.OutputMessage(this.NToolId, MessageStatus.STATUS_Complete, "");
            this.OutputHelper?.Close();
            return true;
        }

        public void PI_Close(bool bHasErrors)
        {
            this.Engine?.OutputMessage(this.NToolId, MessageStatus.STATUS_Info, "PI_Close Called");

            this.OutputHelper = null;
            this.XmlConfig = null;
            this.Engine = null;
            this.NToolId = 0;
        }

        /// <summary>
        /// Show Debug Error Messages
        /// </summary>
        /// <returns></returns>
#if DEBUG
        public bool ShowDebugMessages() => true;
#else
        public bool ShowDebugMessages() => false;
#endif
    }
}
