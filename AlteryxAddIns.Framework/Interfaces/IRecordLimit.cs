namespace JDunkerley.AlteryxAddIns.Framework.Interfaces
{
    /// <summary>
    /// Interface for configuration objects which have a record limit property
    /// </summary>
    public interface IRecordLimit
    {
        /// <summary>
        /// Gets the maximum number of records to send, or 0 for unlimited.
        /// </summary>
        long RecordLimit { get; }
    }
}
