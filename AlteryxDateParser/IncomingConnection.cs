using System;
using System.Xml;
using AlteryxRecordInfoNet;

namespace JDunkerley.Alteryx
{
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

        public XmlElement II_GetPresortXml(XmlElement pXmlProperties)
        {
            throw new NotImplementedException();
        }

        public void II_Close()
        {
            throw new NotImplementedException();
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

        /// <summary>
        /// Show Debug Messages
        /// </summary>
        /// <returns></returns>
        public bool ShowDebugMessages() => this.ParentPlugin.ShowDebugMessages();
    }
}
