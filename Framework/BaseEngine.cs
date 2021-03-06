using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

using AlteryxRecordInfoNet;

using OmniBus.Framework.Factories;
using OmniBus.Framework.Interfaces;
using OmniBus.Framework.Serialisation;

namespace OmniBus.Framework
{
    /// <summary>
    ///     Base Implementation of an <see cref="AlteryxRecordInfoNet.INetPlugin" />
    /// </summary>
    /// <typeparam name="TConfig">Configuration object for reading XML into.</typeparam>
    public abstract class BaseEngine<TConfig> : INetPlugin, IBaseEngine
        where TConfig : new()
    {
        private readonly IOutputHelperFactory _outputHelperFactory;

        private readonly Dictionary<string, PropertyInfo> _inputs;

        private readonly Dictionary<string, PropertyInfo> _outputs;

        private Lazy<TConfig> _configObject;

        private XmlElement _xmlConfig;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseEngine{T}" /> class.
        /// </summary>
        protected BaseEngine()
            : this(new RecordCopierFactory(), new OutputHelperFactory())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseEngine{T}" /> class.
        /// </summary>
        /// <param name="recordCopierFactory">Factory to create copiers</param>
        /// <param name="outputHelperFactory">Factory to create output helpers</param>
        protected BaseEngine(IRecordCopierFactory recordCopierFactory, IOutputHelperFactory outputHelperFactory)
        {
            this.RecordCopierFactory = recordCopierFactory;
            this._outputHelperFactory = outputHelperFactory;

            var type = this.GetType();
            this._inputs = type.GetProperties<IIncomingConnectionInterface>();
            this._outputs = type.GetProperties<IOutputHelper>();

            if (this._outputHelperFactory == null && this._outputs.Count > 0)
            {
                throw new ArgumentNullException(
                    nameof(outputHelperFactory),
                    "Tool has an output but no factory has been provided.");
            }
        }

        /// <summary>
        ///     Gets the Alteryx engine.
        /// </summary>
        public EngineInterface Engine { get; private set; }

        /// <summary>
        ///     Gets the tool identifier. Set at PI_Init, unset at PI_Close.
        /// </summary>
        public int NToolId { get; private set; }

        /// <summary>
        ///     Gets the XML configuration from the work flow.
        /// </summary>
        public XmlElement XmlConfig
        {
            get => this._xmlConfig;

            private set
            {
                this._xmlConfig = value;
                this._configObject = new Lazy<TConfig>(this.CreateConfigObject);
            }
        }

        /// <summary>
        ///     Gets the factory to create RecordCopiers
        /// </summary>
        protected IRecordCopierFactory RecordCopierFactory { get; }

        /// <summary>
        ///     Gets the configuration object read from the XML node.
        /// </summary>
        /// <returns>Configuration Object</returns>
        protected TConfig ConfigObject => this._configObject.Value;

        /// <summary>
        ///     Called by Alteryx to initialize the plug in with configuration info.
        /// </summary>
        /// <param name="nToolId">Tool ID</param>
        /// <param name="engineInterface">Connection to Alteryx Engine (for logging and so on).</param>
        /// <param name="pXmlProperties">Configuration details</param>
        public void PI_Init(int nToolId, EngineInterface engineInterface, XmlElement pXmlProperties)
        {
            this.NToolId = nToolId;
            this.Engine = engineInterface;

            this.XmlConfig = pXmlProperties;

            foreach (var kvp in this._outputs)
            {
                kvp.Value.SetValue(this, this._outputHelperFactory.CreateOutputHelper(this, kvp.Key), null);
            }

            this.OnInitCalled();

            this.DebugMessage("PI_Init Called");
        }

        /// <summary>
        ///     The PI_AddIncomingConnection function pointed to by this property will be called by the Alteryx Engine when an
        ///     incoming data connection is being made.
        /// </summary>
        /// <param name="pIncomingConnectionType">The name of connection given in GetInputConnections.</param>
        /// <param name="pIncomingConnectionName">The name the user gave the connection.</param>
        /// <returns>An <see cref="AlteryxRecordInfoNet.IIncomingConnectionInterface" /> set up to handle the connection.</returns>
        public virtual IIncomingConnectionInterface PI_AddIncomingConnection(
            string pIncomingConnectionType,
            string pIncomingConnectionName)
        {
            if (!this._inputs.TryGetValue(pIncomingConnectionType, out PropertyInfo prop))
            {
                throw new KeyNotFoundException($"Unable to find input {pIncomingConnectionType}");
            }

            var input = prop.GetValue(this, null) as IIncomingConnectionInterface;
            if (input == null)
            {
                throw new NullReferenceException($"{prop.Name} is null.");
            }

            return input;
        }

        /// <summary>
        ///     The PI_AddOutgoingConnection function pointed to by this property will be called by the Alteryx Engine when an
        ///     outgoing data connection is being made.
        /// </summary>
        /// <param name="pOutgoingConnectionName">
        ///     The name will be the name that you gave the connection in the
        ///     IPlugin.GetOutputConnections() method.
        /// </param>
        /// <param name="outgoingConnection">You will need to use the OutgoingConnection object to send your data downstream.</param>
        /// <returns>True if the connection was handled successfully, false otherwise.</returns>
        public virtual bool PI_AddOutgoingConnection(
            string pOutgoingConnectionName,
            OutgoingConnection outgoingConnection)
        {
            if (this._outputs.TryGetValue(pOutgoingConnectionName, out PropertyInfo prop)
                && prop.GetValue(this, null) is OutputHelper helper)
            {
                helper.AddConnection(outgoingConnection);
                this.DebugMessage($"Added Outgoing Connection {pOutgoingConnectionName}");
                return true;
            }

            return false;
        }

        /// <summary>
        ///     The PI_PushAllRecords function pointed to by this property will be called by the Alteryx Engine when the plugin
        ///     should provide all of it's data to the downstream tools.
        ///     This is only pertinent to tools which have no upstream (input) connections (such as the Input tool).
        /// </summary>
        /// <param name="nRecordLimit">
        ///     The nRecordLimit parameter will be &lt; 0 to indicate that there is no limit, 0 to indicate
        ///     that the tool is being configured and no records should be sent, or &gt; 0 to indicate that only the requested
        ///     number of records should be sent.
        /// </param>
        /// <returns>Return true to indicate you successfully handled the request.</returns>
        public virtual bool PI_PushAllRecords(long nRecordLimit)
        {
            return true;
        }

        /// <summary>
        ///     The PI_Close function pointed to by this property will be called by the Alteryx Engine just prior to the
        ///     destruction of the tool object.
        ///     This is guaranteed to happen after all data has finished flowing through all the fields.
        /// </summary>
        /// <param name="bHasErrors">If the bHasErrors parameter is set to true, you would typically not do the final processing.</param>
        public void PI_Close(bool bHasErrors)
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
        ///     Tells Alteryx whether to show debug error messages or not.
        /// </summary>
        /// <returns>A value indicating whether to show debug error messages or not.</returns>
        public virtual bool ShowDebugMessages() => false;

        /// <summary>
        ///     Tell Alteryx execution is complete.
        /// </summary>
        public void ExecutionComplete()
        {
            this.DebugMessage("Output Complete.");
            this.Message(string.Empty, MessageStatus.STATUS_Complete);
        }

        /// <summary>
        ///     Sends a Debug message to the Alteryx log window.
        /// </summary>
        /// <param name="message">Message text.</param>
        public void DebugMessage(string message)
        {
            if (this.ShowDebugMessages())
            {
                this.Message(message);
            }
        }

        /// <summary>
        ///     Sends a message to the Alteryx log window.
        /// </summary>
        /// <param name="message">Message text.</param>
        /// <param name="messageStatus">Type of Message (defaults to Info)</param>
        public void Message(string message, MessageStatus messageStatus = MessageStatus.STATUS_Info)
        {
            this.Engine?.OutputMessage(this.NToolId, messageStatus, message);
        }

        /// <summary>
        ///     Called after <see cref="PI_Init" /> is done.
        /// </summary>
        protected virtual void OnInitCalled()
        {
        }

        /// <summary>
        /// Create a Serialiser
        /// </summary>
        /// <returns><see cref="ISerialiser{TConfig}"/> to de-serialise object</returns>
        protected virtual ISerialiser<TConfig> Serialiser()
            => new XmlSerialiser<TConfig>();

        private TConfig CreateConfigObject()
        {
            var config = this.XmlConfig.SelectSingleNode("Configuration");
            return this.Serialiser().Deserialise(config);
        }
    }
}