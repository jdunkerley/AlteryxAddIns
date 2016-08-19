namespace JDunkerley.AlteryxAddIns.Framework.Interfaces
{
    /// <summary>
    /// Interface for creation of RecordCopier
    /// </summary>
    public interface IRecordCopierFactory
    {
        /// <summary>
        /// Creates a record copier copier.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="newRecordInfo">The new record information.</param>
        /// <param name="fieldsToSkip">The fields to skip.</param>
        /// <returns></returns>
        IRecordCopier CreateCopier(
            AlteryxRecordInfoNet.RecordInfo info,
            AlteryxRecordInfoNet.RecordInfo newRecordInfo,
            params string[] fieldsToSkip);
    }
}