namespace JDunkerley.Alteryx
{
    using System;
    using System.Xml;
    using AlteryxRecordInfoNet;

    /// <summary>
    /// Wrap An Incoming Connection
    /// </summary>
    public class IncomingConnection : IIncomingConnectionInterface
    {
        private INetPlugin ParentPlugin { get; }

        /// <summary>
        /// Create an Incoming Connection
        /// </summary>
        /// <param name="parentPlugin"></param>
        public IncomingConnection(INetPlugin parentPlugin)
        {
            this.ParentPlugin = parentPlugin;
        }

        /// <summary>
        /// is the i_ get presort XML.
        /// </summary>
        /// <param name="pXmlProperties">The p XML properties.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public XmlElement II_GetPresortXml(XmlElement pXmlProperties)
        {
            return null;
        }

        public bool II_Init(RecordInfo recordInfo)
        {
            throw new NotImplementedException();
        }

        public bool II_PushRecord(RecordData pRecord)
        {
            throw new NotImplementedException();
        }

        public void II_UpdateProgress(double dPercent)
        {
            throw new NotImplementedException();
        }

        public void II_Close()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Show Debug Messages
        /// </summary>
        /// <returns></returns>
        public bool ShowDebugMessages() => this.ParentPlugin.ShowDebugMessages();
    }
}
