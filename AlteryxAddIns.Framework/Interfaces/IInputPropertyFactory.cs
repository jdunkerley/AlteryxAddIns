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
        IInputProperty Build(
            IRecordCopierFactory copierFactory = null,
            Func<bool> showDebugMessagesFunc = null,
            Func<XmlElement, IEnumerable<string>> sortFieldsFunc = null,
            Func<XmlElement, IEnumerable<string>> selectFieldsFunc = null);
    }
}