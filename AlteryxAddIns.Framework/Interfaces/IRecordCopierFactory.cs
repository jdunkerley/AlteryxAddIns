namespace JDunkerley.AlteryxAddIns.Framework.Interfaces
{
    /// <summary>
    /// Interface to decouple the construction of <see cref="IRecordCopier"/> objects from the engines.
    /// </summary>
    public interface IRecordCopierFactory
    {
        /// <summary>
        /// Creates a new instance of an <see cref="IRecordCopier"/>.
        /// </summary>
        /// <param name="info">The source <see cref="AlteryxRecordInfoNet.RecordInfo"/> object.</param>
        /// <param name="newRecordInfo">The target <see cref="AlteryxRecordInfoNet.RecordInfo"/> objects.</param>
        /// <param name="fieldsToSkip">A list of fields to skip.</param>
        /// <returns>A new instance of an <see cref="IRecordCopier"/> object.</returns>
        IRecordCopier CreateCopier(
            AlteryxRecordInfoNet.RecordInfo info,
            AlteryxRecordInfoNet.RecordInfo newRecordInfo,
            params string[] fieldsToSkip);
    }
}