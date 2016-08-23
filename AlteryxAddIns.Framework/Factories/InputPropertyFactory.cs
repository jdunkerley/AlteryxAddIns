namespace JDunkerley.AlteryxAddIns.Framework.Factories
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    using JDunkerley.AlteryxAddIns.Framework.Interfaces;

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
        /// <returns></returns>
        public IInputProperty Build(
            IRecordCopierFactory copierFactory = null,
            Func<bool> showDebugMessagesFunc = null,
            Func<XmlElement, IEnumerable<string>> sortFieldsFunc = null,
            Func<XmlElement, IEnumerable<string>> selectFieldsFunc = null)
        {
            return new InputProperty(copierFactory, showDebugMessagesFunc, sortFieldsFunc, selectFieldsFunc);
        }
    }
}