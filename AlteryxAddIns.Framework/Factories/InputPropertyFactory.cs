namespace JDunkerley.AlteryxAddIns.Framework.Factories
{
    using System;

    using Interfaces;

    /// <summary>
    /// Factory For Creating <see cref="InputProperty"/> objects.
    /// </summary>
    public class InputPropertyFactory : IInputPropertyFactory
    {
        /// <summary>
        /// Creates a new instance of an <see cref="InputProperty"/> object.
        /// </summary>
        /// <param name="copierFactory">Factory for creating RecordCopiers</param>
        /// <param name="showDebugMessagesFunc">Call back to determine whether to show debug messages</param>
        /// <returns>A new instance of an <see cref="InputProperty"/>.</returns>
        public IInputProperty Build(
            IRecordCopierFactory copierFactory = null,
            Func<bool> showDebugMessagesFunc = null)
        {
            return new InputProperty(copierFactory, showDebugMessagesFunc);
        }
    }
}