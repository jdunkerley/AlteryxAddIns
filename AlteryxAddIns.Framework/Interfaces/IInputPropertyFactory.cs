namespace JDunkerley.AlteryxAddIns.Framework.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    public interface IInputPropertyFactory
    {
        /// <summary>
        /// Creates a new instance of an <see cref="IInputProperty"/>.
        /// </summary>
        /// <param name="copierFactory">Factory for creating RecordCopiers</param>
        /// <param name="showDebugMessagesFunc">Call back to determine whether to show debug messages</param>
        /// <param name="sortFieldsFunc">The sort fields function.</param>
        /// <param name="selectFieldsFunc">The select fields function.</param>
        /// <param name="initFunc">The initialize function.</param>
        /// <param name="pushFunc">The push function.</param>
        /// <param name="progressAction">The progress action.</param>
        /// <param name="closedAction">The closed action.</param>
        IInputProperty Build(
            IRecordCopierFactory copierFactory = null,
            Func<bool> showDebugMessagesFunc = null,
            Func<XmlElement, IEnumerable<string>> sortFieldsFunc = null,
            Func<XmlElement, IEnumerable<string>> selectFieldsFunc = null,
            Func<AlteryxRecordInfoNet.RecordInfo, bool> initFunc = null,
            Func<AlteryxRecordInfoNet.RecordData, bool> pushFunc = null,
            Action<double> progressAction = null,
            Action closedAction = null);
    }
}