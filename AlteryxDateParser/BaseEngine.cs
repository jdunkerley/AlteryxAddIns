namespace JDunkerley.Alteryx
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Serialization;

    using AlteryxRecordInfoNet;

    public abstract class BaseEngine<T> : INetPlugin, IBaseEngine
        where T: new()
    {
        private readonly Dictionary<string, PropertyInfo> _inputs;
        private readonly Dictionary<string, PropertyInfo> _outputs;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseEngine{T}"/> class.
        /// </summary>
        protected BaseEngine()
        {
            this._inputs = this.GetType().GetConnections<IncomingConnection>();
            this._outputs = this.GetType().GetConnections<PluginOutputConnectionHelper>();
        }

        /// <summary>
        /// Gets the Alteryx engine.
        /// </summary>
        protected EngineInterface Engine { get; private set; }

        /// <summary>
        /// Gets the tool identifier. Set at PI_Init, unset at PI_Close.
        /// </summary>
        protected int NToolId { get; private set; }

        /// <summary>
        /// Gets the XML configuration from the workflow.
        /// </summary>
        protected XmlElement XmlConfig { get; private set; }

        /// <summary>
        /// Gets the configuration object de-serialized from the XML config
        /// </summary>
        /// <returns>Configuration Object</returns>
        protected T GetConfigObject()
        {
            var serializer = new XmlSerializer(typeof(T));
            var config = this.XmlConfig.SelectSingleNode("Configuration");
            if (config == null)
            {
                return new T();
            }

            var doc = new XmlDocument();
            doc.LoadXml($"<Config>{config.InnerXml}</Config>");
            if (doc.DocumentElement == null)
            {
                return new T();
            }

            return (T)serializer.Deserialize(new XmlNodeReader(doc.DocumentElement));
        }

        /// <summary>
        /// Called by Alteryx to initialize the plug in with configuration info.
        /// </summary>
        /// <param name="nToolId">Tool ID</param>
        /// <param name="engineInterface">Connection to Alteryx Engine (for logging etc)</param>
        /// <param name="pXmlProperties">Configuration details</param>
        public void PI_Init(int nToolId, EngineInterface engineInterface, XmlElement pXmlProperties)
        {
            this.NToolId = nToolId;
            this.Engine = engineInterface;

            this.XmlConfig = pXmlProperties;

            foreach (var kvp in this._outputs)
            {
                kvp.Value.SetValue(this, new PluginOutputConnectionHelper(this.NToolId, this.Engine), null);
            }

#if DEBUG
            this.Engine?.OutputMessage(this.NToolId, MessageStatus.STATUS_Info, "PI_Init Called");
#endif
        }

        /// <summary>
        /// Handle Incoming Connections Being Added
        /// </summary>
        /// <param name="pIncomingConnectionType"></param>
        /// <param name="pIncomingConnectionName"></param>
        /// <returns></returns>
        public virtual IIncomingConnectionInterface PI_AddIncomingConnection(string pIncomingConnectionType, string pIncomingConnectionName)
        {
            PropertyInfo prop;
            if (!this._inputs.TryGetValue(pIncomingConnectionType, out prop))
            {
                throw new KeyNotFoundException($"Unable to find input {pIncomingConnectionType}");
            }

            var connection = new IncomingConnection(prop.Name, this);
            prop.SetValue(this, connection, null);
            return connection;
        }

        /// <summary>
        /// Handle Outgoing Connections Being Added
        /// </summary>
        /// <param name="pOutgoingConnectionName"></param>
        /// <param name="outgoingConnection"></param>
        /// <returns></returns>
        public virtual bool PI_AddOutgoingConnection(string pOutgoingConnectionName, OutgoingConnection outgoingConnection)
        {
            PropertyInfo prop;
            if (!this._outputs.TryGetValue(pOutgoingConnectionName, out prop))
            {
                return false;
            }

            var helper = prop.GetValue(this, null) as PluginOutputConnectionHelper;
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
        public virtual bool PI_PushAllRecords(long nRecordLimit) => true;

        /// <summary>
        /// Called by Alteryx to close the tool
        /// </summary>
        /// <param name="bHasErrors">if set to <c>true</c> [b has errors].</param>
        public virtual void PI_Close(bool bHasErrors)
        {
            this.Engine?.OutputMessage(this.NToolId, MessageStatus.STATUS_Info, "PI_Close Called");

            foreach (var kvp in this._outputs)
            {
                kvp.Value.SetValue(this, null, null);
            }

            this.XmlConfig = null;
            this.Engine = null;
            this.NToolId = 0;
        }

        /// <summary>
        /// Field Names to Sort By. Prefix with ~ for Descending.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="pXmlProperties">The XML COnfiguration Properties</param>
        /// <returns>Sort Fields</returns>
        public virtual IEnumerable<string> IncomingConnectionSort(string name, XmlElement pXmlProperties) => null;

        /// <summary>
        /// Field Names to Select
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="pXmlProperties">The XML COnfiguration Properties</param>
        /// <returns>Selected Fields or NULL for all</returns>
        public virtual IEnumerable<string> IncomingConnectionFields(string name, XmlElement pXmlProperties) => null;

        /// <summary>
        /// Alteryx Initialized An Incoming Connection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public virtual bool IncomingConnectionInit(string name) => true;

        /// <summary>
        /// Called by Alteryx to send each data record to the tool.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="record">The new record</param>
        /// <returns></returns>
        public virtual bool IncomingConnectionPush(string name, RecordData record) => true;

        /// <summary>
        /// Called by Alteryx to update progress
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="progress">Progress (0 to 1)</param>
        public virtual void IncomingConnectionProgress(string name, double progress)
        {
        }

        /// <summary>
        /// Alteryx Finished Sending Data For An Incoming Connection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public virtual void IncomingConnectionClosed(string name)
        {
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