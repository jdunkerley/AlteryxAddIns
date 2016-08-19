namespace JDunkerley.AlteryxAddIns.Framework.Interfaces
{
    /// <summary>
    /// Incoming Connection Engine Interface
    /// </summary>
    public interface IInputProperty: AlteryxRecordInfoNet.IIncomingConnectionInterface
    {
        /// <summary>
        /// Gets the current state.
        /// </summary>
        ConnectionState State { get; }

        /// <summary>
        /// Gets the record information of incoming stream.
        /// </summary>
        AlteryxRecordInfoNet.RecordInfo RecordInfo { get; }

        /// <summary>
        /// Gets the record copier for this property.
        /// </summary>
        IRecordCopier Copier { get; }
    }
}