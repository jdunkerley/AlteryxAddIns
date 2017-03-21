using System;

namespace OmniBus.Framework.Interfaces
{
    /// <summary>
    ///     Interface to decouple the construction of <see cref="IInputProperty" /> objects from the engines.
    /// </summary>
    public interface IInputPropertyFactory
    {
        /// <summary>
        ///     Creates a new instance of an <see cref="IInputProperty" />.
        /// </summary>
        /// <param name="copierFactory">Factory for creating RecordCopiers</param>
        /// <param name="showDebugMessagesFunc">Call back to determine whether to show debug messages</param>
        /// <returns>A new instance of an <see cref="IInputProperty" /></returns>
        IInputProperty Build(IRecordCopierFactory copierFactory = null, Func<bool> showDebugMessagesFunc = null);
    }
}