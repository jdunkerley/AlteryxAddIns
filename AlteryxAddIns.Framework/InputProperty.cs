namespace JDunkerley.AlteryxAddIns.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    using AlteryxRecordInfoNet;

    /// <summary>
    /// Handle the connection to Alteryx.
    /// Responds to the interface
    /// Has a state property
    /// </summary>
    /// <seealso cref="IInputProperty" />
    public class InputProperty : IInputProperty
    {
        private readonly Func<XmlElement, IEnumerable<string>> _sortFieldsFunc;

        private readonly Func<XmlElement, IEnumerable<string>> _selectFieldsFunc;

        private readonly Func<RecordInfo, bool> _initFunc;

        private readonly Func<RecordData, bool> _pushFunc;

        private readonly Action<double> _progressAction;

        private readonly Action _closedAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputProperty"/> class.
        /// </summary>
        /// <param name="sortFieldsFunc">The sort fields function.</param>
        /// <param name="selectFieldsFunc">The select fields function.</param>
        /// <param name="initFunc">The initialize function.</param>
        /// <param name="pushFunc">The push function.</param>
        /// <param name="progressAction">The progress action.</param>
        /// <param name="closedAction">The closed action.</param>
        public InputProperty(
            Func<XmlElement, IEnumerable<string>> sortFieldsFunc = null,
            Func<XmlElement, IEnumerable<string>> selectFieldsFunc = null,
            Func<RecordInfo, bool> initFunc = null,
            Func<RecordData, bool> pushFunc = null,
            Action<double> progressAction = null,
            Action closedAction = null)
        {
            this._sortFieldsFunc = sortFieldsFunc ?? (p => null);
            this._selectFieldsFunc = selectFieldsFunc ?? (p => null);
            this._initFunc = initFunc ?? (i => true);
            this._pushFunc = pushFunc ?? (r => true);
            this._progressAction = progressAction;
            this._closedAction = closedAction;
        }

        /// <summary>
        /// Engine Hosting The Property
        /// </summary>
        public INetPlugin Engine { get; set; }

        /// <summary>
        /// Gets the current state.
        /// </summary>
        public ConnectionState State { get; private set; }

        /// <summary>
        /// Gets the record information of incoming stream.
        /// </summary>
        public RecordInfo RecordInfo { get; private set; }

        private Lazy<RecordCopier> CopierLazy { get; set; }

        /// <summary>
        /// Gets the copier, set up to copy all fields.
        /// </summary>
        public RecordCopier Copier => this.CopierLazy.Value;

        /// <summary>
        /// Called by Alteryx to determine if the incoming data should be sorted.
        /// </summary>
        /// <param name="pXmlProperties">The XML COnfiguration Properties</param>
        /// <returns>Null To  Do Nothing, Xml To Sort or Filter Columns</returns>
        XmlElement IIncomingConnectionInterface.II_GetPresortXml(XmlElement pXmlProperties)
        {
            this.State = ConnectionState.Added;
            var sortFields = this.IncomingConnectionSort(pXmlProperties);
            var selectFields = this.IncomingConnectionFields(pXmlProperties);

            // ToDo: Render Xml Output
            return null;

        }

        /// <summary>
        /// Field Names to Sort By. Prefix with ~ for Descending.
        /// </summary>
        /// <param name="pXmlProperties">The XML COnfiguration Properties</param>
        /// <returns>Sort Fields</returns>
        public IEnumerable<string> IncomingConnectionSort(XmlElement pXmlProperties)
            => this._sortFieldsFunc(pXmlProperties);

        /// <summary>
        /// Field Names to Select
        /// </summary>
        /// <param name="pXmlProperties">The XML COnfiguration Properties</param>
        /// <returns>Selected Fields or NULL for all</returns>
        public IEnumerable<string> IncomingConnectionFields(XmlElement pXmlProperties)
            => this._selectFieldsFunc(pXmlProperties);

        /// <summary>
        /// Called by Alteryx to initialize the incoming connection.
        /// </summary>
        /// <param name="recordInfo">The record information.</param>
        /// <returns>True if OK</returns>
        public virtual bool II_Init(RecordInfo recordInfo)
        {
            this.State = ConnectionState.InitCalled;
            this.RecordInfo = recordInfo;

            this.CopierLazy = new Lazy<RecordCopier>(() => Utilities.CreateCopier(this.RecordInfo, this.RecordInfo));
            return this._initFunc(recordInfo);
        }

        /// <summary>
        /// Called by Alteryx to send each data record to the tool.
        /// </summary>
        /// <param name="pRecord">The new record</param>
        /// <returns>True if Ok</returns>
        public virtual bool II_PushRecord(RecordData pRecord) => this._pushFunc(pRecord);

        /// <summary>
        /// Called by Alteryx when it wants the tool to update its progress.
        /// </summary>
        /// <param name="dPercent">The new progress</param>
        public virtual void II_UpdateProgress(double dPercent)
            => this._progressAction?.Invoke(dPercent);

        /// <summary>
        /// Called by Alteryx when the connection is finished sending data.
        /// </summary>
        public virtual void II_Close()
        {
            this.State = ConnectionState.Closed;
            this._closedAction?.Invoke();
        }

        public bool ShowDebugMessages() => this.Engine?.ShowDebugMessages() ?? false;
    }
}