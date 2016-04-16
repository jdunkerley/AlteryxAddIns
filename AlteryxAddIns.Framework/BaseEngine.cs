namespace JDunkerley.AlteryxAddIns.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Serialization;

    public abstract class BaseEngine<T> : AlteryxRecordInfoNet.INetPlugin, IBaseEngine
        where T: new()
    {
        private readonly Dictionary<string, PropertyInfo> _inputs;
        private readonly Dictionary<string, PropertyInfo> _outputs;

        private Lazy<T> _configObject;

        private XmlElement _xmlConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseEngine{T}"/> class.
        /// </summary>
        protected BaseEngine()
        {
            this._inputs = this.GetType().GetConnections<AlteryxRecordInfoNet.IIncomingConnectionInterface>();
            this._outputs = this.GetType().GetConnections<OutputHelper>();
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
                this._configObject = new Lazy<T>(this.CreateConfigObject);
            }
        }

        /// <summary>
        /// Gets the configuration object de-serialized from the XML config
        /// </summary>
        /// <returns>Configuration Object</returns>
        protected T ConfigObject => this._configObject.Value;

        private T CreateConfigObject()
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

            var inputProp = (input as IInputProperty);
            if (inputProp != null)
            {
                inputProp.Engine = this;
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
            this.Engine?.OutputMessage(this.NToolId, AlteryxRecordInfoNet.MessageStatus.STATUS_Complete, "");
        }

        public void DebugMessage(string message)
        {
            if (this.ShowDebugMessages())
            {
                this.Engine?.OutputMessage(this.NToolId, AlteryxRecordInfoNet.MessageStatus.STATUS_Info, message);
            }
        }
    }
}