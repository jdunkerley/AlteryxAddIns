namespace JDunkerley.Alteryx
{
    using System.Xml;
    using AlteryxRecordInfoNet;

    /// <summary>
    /// Wrap An Incoming Connection
    /// </summary>
    public class IncomingConnection : IIncomingConnectionInterface
    {
        private string Name { get; }

        private IBaseEngine ParentPlugin { get; }

        /// <summary>
        /// Create an Incoming Connection
        /// </summary>
        /// <param name="name">Connection Name</param>
        /// <param name="parentPlugin">PlugIn Engine</param>
        public IncomingConnection(string name, IBaseEngine parentPlugin)
        {
            this.Name = name;
            this.ParentPlugin = parentPlugin;
        }

        /// <summary>
        /// Gets the record information of incoming stream.
        /// </summary>
        public RecordInfo RecordInfo { get; private set; }

        /// <summary>
        /// Called by Alteryx to determine if the incoming data should be sorted.
        /// </summary>
        /// <param name="pXmlProperties">The XML COnfiguration Properties</param>
        /// <returns>Null To  Do Nothing, Xml To Sort or Filter Columns</returns>
        public XmlElement II_GetPresortXml(XmlElement pXmlProperties)
        {
            var sortFields = this.ParentPlugin.IncomingConnectionSort(this.Name, pXmlProperties);
            var selectFields = this.ParentPlugin.IncomingConnectionFields(this.Name, pXmlProperties);
            return null;
        }

        /// <summary>
        /// Called by Alteryx to initialize the incoming connection.
        /// </summary>
        /// <param name="recordInfo">The record information.</param>
        /// <returns>True if OK</returns>
        public bool II_Init(RecordInfo recordInfo)
        {
            this.RecordInfo = recordInfo;
            return this.ParentPlugin.IncomingConnectionInit(this.Name);
        }

        /// <summary>
        /// Called by Alteryx to send each data record to the tool.
        /// </summary>
        /// <param name="pRecord">The new record</param>
        /// <returns>True if Ok</returns>
        public bool II_PushRecord(RecordData pRecord) => this.ParentPlugin.IncomingConnectionPush(this.Name, pRecord);

        /// <summary>
        /// Called by Alteryx when it wants the tool to update its progress.
        /// </summary>
        /// <param name="dPercent">The new progress</param>
        public void II_UpdateProgress(double dPercent) => this.ParentPlugin.IncomingConnectionProgress(this.Name, dPercent);

        /// <summary>
        /// Called by Alteryx when the connection is finished sending data.
        /// </summary>
        public void II_Close() => this.ParentPlugin.IncomingConnectionClosed(this.Name);
 
        /// <summary>
        /// Show Debug Messages
        /// </summary>
        /// <returns></returns>
        public bool ShowDebugMessages() => this.ParentPlugin.ShowDebugMessages();
    }
}
