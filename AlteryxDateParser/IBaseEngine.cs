namespace JDunkerley.Alteryx
{
    using System.Collections.Generic;
    using System.Xml;

    using AlteryxRecordInfoNet;

    /// <summary>
    /// Incoming Connection Engine Interface
    /// </summary>
    public interface IBaseEngine
    {
        /// <summary>
        /// Field Names to Sort By. Prefix with ~ for Descending.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="pXmlProperties">The XML COnfiguration Properties</param>
        /// <returns>Sort Fields</returns>
        IEnumerable<string> IncomingConnectionSort(string name, XmlElement pXmlProperties);

        /// <summary>
        /// Field Names to Select
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="pXmlProperties">The XML COnfiguration Properties</param>
        /// <returns>Selected Fields or NULL for all</returns>
        IEnumerable<string> IncomingConnectionFields(string name, XmlElement pXmlProperties);

        /// <summary>
        /// Alteryx Initialized An Incoming Connection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        bool IncomingConnectionInit(string name);

        /// <summary>
        /// Called by Alteryx to send each data record to the tool.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="record">The new record</param>
        /// <returns></returns>
        bool IncomingConnectionPush(string name, RecordData record);

        /// <summary>
        /// Called by Alteryx to update progress
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="progress">Progress (0 to 1)</param>
        void IncomingConnectionProgress(string name, double progress);

        /// <summary>
        /// Alteryx Finished Sending Data For An Incoming Connection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        void IncomingConnectionClosed(string name);

        /// <summary>
        /// Show Debug Error Messages
        /// </summary>
        /// <returns></returns>
        bool ShowDebugMessages();
    }
}