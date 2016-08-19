namespace JDunkerley.AlteryxAddIns.Framework.Factories
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    using AlteryxRecordInfoNet;

    using Interfaces;

    /// <summary>
    /// Factory For Creating Input Properties
    /// </summary>
    public class InputPropertyFactory : IInputPropertyFactory
    {
        /// <summary>
        /// Given a set of call back functions create the input property
        /// </summary>
        /// <param name="copierFactory"></param>
        /// <param name="showDebugMessagesFunc"></param>
        /// <param name="sortFieldsFunc"></param>
        /// <param name="selectFieldsFunc"></param>
        /// <param name="initFunc"></param>
        /// <param name="pushFunc"></param>
        /// <param name="progressAction"></param>
        /// <param name="closedAction"></param>
        /// <returns></returns>
        public IInputProperty Build(
            IRecordCopierFactory copierFactory = null,
            Func<bool> showDebugMessagesFunc = null,
            Func<XmlElement, IEnumerable<string>> sortFieldsFunc = null,
            Func<XmlElement, IEnumerable<string>> selectFieldsFunc = null,
            Func<RecordInfo, bool> initFunc = null,
            Func<RecordData, bool> pushFunc = null,
            Action<double> progressAction = null,
            Action closedAction = null)
        {
            return new InputProperty(copierFactory, showDebugMessagesFunc, sortFieldsFunc, selectFieldsFunc, initFunc, pushFunc, progressAction, closedAction);
        }

        /// <summary>
        /// Handle the connection to Alteryx.
        /// Responds to the interface
        /// Has a state property
        /// </summary>
        /// <seealso cref="IInputProperty" />
        private sealed class InputProperty : IInputProperty
        {
            private readonly Func<bool> _showDebugMessagesFunc;

            private readonly Func<XmlElement, IEnumerable<string>> _sortFieldsFunc;

            private readonly Func<XmlElement, IEnumerable<string>> _selectFieldsFunc;

            private readonly Func<AlteryxRecordInfoNet.RecordInfo, bool> _initFunc;

            private readonly Func<AlteryxRecordInfoNet.RecordData, bool> _pushFunc;

            private readonly Action<double> _progressAction;

            private readonly Action _closedAction;

            private readonly Lazy<IRecordCopier> _lazyCopier;

            /// <summary>
            /// Initializes a new instance of the <see cref="InputProperty"/> class.
            /// </summary>
            /// <param name="copierFactory">Factory for creating RecordCopiers</param>
            /// <param name="showDebugMessagesFunc">Call back to determine whether to show debug messages</param>
            /// <param name="sortFieldsFunc">The sort fields function.</param>
            /// <param name="selectFieldsFunc">The select fields function.</param>
            /// <param name="initFunc">The initialize function.</param>
            /// <param name="pushFunc">The push function.</param>
            /// <param name="progressAction">The progress action.</param>
            /// <param name="closedAction">The closed action.</param>
            internal InputProperty(
                IRecordCopierFactory copierFactory = null,
                Func<bool> showDebugMessagesFunc = null,
                Func<XmlElement, IEnumerable<string>> sortFieldsFunc = null,
                Func<XmlElement, IEnumerable<string>> selectFieldsFunc = null,
                Func<AlteryxRecordInfoNet.RecordInfo, bool> initFunc = null,
                Func<AlteryxRecordInfoNet.RecordData, bool> pushFunc = null,
                Action<double> progressAction = null,
                Action closedAction = null)
            {
                this._lazyCopier = new Lazy<IRecordCopier>(() => copierFactory?.CreateCopier(this.RecordInfo, this.RecordInfo));

                this._showDebugMessagesFunc = showDebugMessagesFunc ?? (() => false);

                this._sortFieldsFunc = sortFieldsFunc ?? (p => null);
                this._selectFieldsFunc = selectFieldsFunc ?? (p => null);
                this._initFunc = initFunc ?? (i => true);
                this._pushFunc = pushFunc ?? (r => true);
                this._progressAction = progressAction;
                this._closedAction = closedAction;
            }

            /// <summary>
            /// Gets the current state.
            /// </summary>
            public ConnectionState State { get; private set; }

            /// <summary>
            /// Gets the record information of incoming stream.
            /// </summary>
            public AlteryxRecordInfoNet.RecordInfo RecordInfo { get; private set; }

            /// <summary>
            /// Gets the record copier for this property.
            /// </summary>
            public IRecordCopier Copier => this._lazyCopier.Value;

            /// <summary>
            /// Field Names to Sort By. Prefix with ~ for Descending.
            /// </summary>
            /// <param name="pXmlProperties">The XML Configuration Properties</param>
            /// <returns>Sort Fields</returns>
            private IEnumerable<string> IncomingConnectionSort(XmlElement pXmlProperties)
                => this._sortFieldsFunc(pXmlProperties);

            /// <summary>
            /// Field Names to Select
            /// </summary>
            /// <param name="pXmlProperties">The XML COnfiguration Properties</param>
            /// <returns>Selected Fields or NULL for all</returns>
            private IEnumerable<string> IncomingConnectionFields(XmlElement pXmlProperties)
                => this._selectFieldsFunc(pXmlProperties);

            /// <summary>
            /// Called by Alteryx to determine if the incoming data should be sorted.
            /// </summary>
            /// <param name="pXmlProperties">The XML COnfiguration Properties</param>
            /// <returns>Null To  Do Nothing, Xml To Sort or Filter Columns</returns>
            XmlElement AlteryxRecordInfoNet.IIncomingConnectionInterface.II_GetPresortXml(XmlElement pXmlProperties)
            {
                this.State = ConnectionState.Added;
                //var sortFields = this.IncomingConnectionSort(pXmlProperties);
                //var selectFields = this.IncomingConnectionFields(pXmlProperties);

                // ToDo: Render Xml Output
                return null;

            }

            /// <summary>
            /// Called by Alteryx to initialize the incoming connection.
            /// </summary>
            /// <param name="recordInfo">The record information.</param>
            /// <returns>True if OK</returns>
            bool AlteryxRecordInfoNet.IIncomingConnectionInterface.II_Init(AlteryxRecordInfoNet.RecordInfo recordInfo)
            {
                this.State = ConnectionState.InitCalled;
                this.RecordInfo = recordInfo;
                return this._initFunc(recordInfo);
            }

            /// <summary>
            /// Called by Alteryx to send each data record to the tool.
            /// </summary>
            /// <param name="pRecord">The new record</param>
            /// <returns>True if Ok</returns>
            bool AlteryxRecordInfoNet.IIncomingConnectionInterface.II_PushRecord(AlteryxRecordInfoNet.RecordData pRecord) => this._pushFunc(pRecord);

            /// <summary>
            /// Called by Alteryx when it wants the tool to update its progress.
            /// </summary>
            /// <param name="dPercent">The new progress</param>
            void AlteryxRecordInfoNet.IIncomingConnectionInterface.II_UpdateProgress(double dPercent)
                => this._progressAction?.Invoke(dPercent);

            /// <summary>
            /// Called by Alteryx when the connection is finished sending data.
            /// </summary>
            void AlteryxRecordInfoNet.IIncomingConnectionInterface.II_Close()
            {
                this.State = ConnectionState.Closed;
                this._closedAction?.Invoke();
            }

            bool AlteryxRecordInfoNet.IIncomingConnectionInterface.ShowDebugMessages() => this._showDebugMessagesFunc();
        }
    }
}