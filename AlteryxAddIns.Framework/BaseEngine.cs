namespace JDunkerley.AlteryxAddIns.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Serialization;

    using Interfaces;

    /// <summary>
    /// Base Implementation of an <see cref="AlteryxRecordInfoNet.INetPlugin"/>
    /// </summary>
    /// <typeparam name="TConfig">Configuration object for de-serialising XML to</typeparam>
    public abstract class BaseEngine<TConfig> : AlteryxRecordInfoNet.INetPlugin, IBaseEngine
        where TConfig : new()
    {
        private readonly Dictionary<string, PropertyInfo> _inputs;
        private readonly Dictionary<string, PropertyInfo> _outputs;

        private Lazy<TConfig> _configObject;

        private XmlElement _xmlConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseEngine{T}"/> class.
        /// </summary>
        /// <param name="recordCopierFactory">Factory to create copiers</param>
        protected BaseEngine(IRecordCopierFactory recordCopierFactory)
        {
            this.RecordCopierFactory = recordCopierFactory;

            var type = this.GetType();
            this._inputs = type.GetConnections<AlteryxRecordInfoNet.IIncomingConnectionInterface>();
            this._outputs = type.GetConnections<OutputHelper>();
        }

        /// <summary>
        /// Gets the Alteryx engine.
        /// </summary>
        public AlteryxRecordInfoNet.EngineInterface Engine { get; private set; }

        /// <summary>
        /// Gets the tool identifier. Set at PI_Init, unset at PI_Close.
        /// </summary>
        public int NToolId { get; private set; }

        /// <summary>
        /// Gets the XML configuration from the workflow.
        /// </summary>
        public XmlElement XmlConfig
        {
            get
            {
                return this._xmlConfig;
            }

            private set
            {
                this._xmlConfig = value;
                this._configObject = new Lazy<TConfig>(this.CreateConfigObject);
            }
        }

        /// <summary>
        /// Gets the factory to create RecordCopiers
        /// </summary>
        protected IRecordCopierFactory RecordCopierFactory { get; }

        /// <summary>
        /// Gets the configuration object de-serialized from the XML config
        /// </summary>
        /// <returns>Configuration Object</returns>
        protected TConfig ConfigObject => this._configObject.Value;

        /// <summary>
        /// Called by Alteryx to initialize the plug in with configuration info.
        /// </summary>
        /// <param name="nToolId">Tool ID</param>
        /// <param name="engineInterface">Connection to Alteryx Engine (for logging etc)</param>
        /// <param name="pXmlProperties">Configuration details</param>
        public void PI_Init(int nToolId, AlteryxRecordInfoNet.EngineInterface engineInterface, XmlElement pXmlProperties)
        {
            this.NToolId = nToolId;
            this.Engine = engineInterface;

            this.XmlConfig = pXmlProperties;

            foreach (var kvp in this._outputs)
            {
                kvp.Value.SetValue(this, new OutputHelper(this, kvp.Key), null);
            }

            this.DebugMessage("PI_Init Called");
        }

        /// <summary>
        /// Handle Incoming Connections Being Added
        /// </summary>
        /// <param name="pIncomingConnectionType"></param>
        /// <param name="pIncomingConnectionName"></param>
        /// <returns></returns>
        public virtual AlteryxRecordInfoNet.IIncomingConnectionInterface PI_AddIncomingConnection(string pIncomingConnectionType, string pIncomingConnectionName)
        {
            PropertyInfo prop;
            if (!this._inputs.TryGetValue(pIncomingConnectionType, out prop))
            {
                throw new KeyNotFoundException($"Unable to find input {pIncomingConnectionType}");
            }

            var input = prop.GetValue(this, null) as AlteryxRecordInfoNet.IIncomingConnectionInterface;
            if (input == null)
            {
                throw new NullReferenceException($"{prop.Name} is null.");
            }

            return input;
        }

        /// <summary>
        /// Handle Outgoing Connections Being Added
        /// </summary>
        /// <param name="pOutgoingConnectionName"></param>
        /// <param name="outgoingConnection"></param>
        /// <returns></returns>
        public virtual bool PI_AddOutgoingConnection(string pOutgoingConnectionName, AlteryxRecordInfoNet.OutgoingConnection outgoingConnection)
        {
            PropertyInfo prop;
            if (!this._outputs.TryGetValue(pOutgoingConnectionName, out prop))
            {
                return false;
            }

            var helper = prop.GetValue(this, null) as OutputHelper;
            if (helper == null)
            {
                return false;
            }

            helper.AddConnection(outgoingConnection);
            this.DebugMessage($"Added Outgoing Connection {pOutgoingConnectionName}");
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
            this.DebugMessage("PI_Close Called.");

            foreach (var kvp in this._outputs)
            {
                kvp.Value.SetValue(this, null, null);
            }

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
        public virtual bool ShowDebugMessages() => false;
#endif

        /// <summary>
        /// Tell Alteryx Is Complete
        /// </summary>
        public void ExecutionComplete()
        {
            this.DebugMessage("Output Complete.");
            this.Engine?.OutputMessage(this.NToolId, AlteryxRecordInfoNet.MessageStatus.STATUS_Complete, string.Empty);
        }

        public void DebugMessage(string message)
        {
            if (this.ShowDebugMessages())
            {
                this.Engine?.OutputMessage(this.NToolId, AlteryxRecordInfoNet.MessageStatus.STATUS_Info, message);
            }
        }

        private TConfig CreateConfigObject()
        {
            var serializer = new XmlSerializer(typeof(TConfig));
            var config = this.XmlConfig.SelectSingleNode("Configuration");
            if (config == null)
            {
                return new TConfig();
            }

            var doc = new XmlDocument();
            doc.LoadXml($"<Config>{config.InnerXml}</Config>");
            if (doc.DocumentElement == null)
            {
                return new TConfig();
            }

            return (TConfig)serializer.Deserialize(new XmlNodeReader(doc.DocumentElement));
        }
    }
}