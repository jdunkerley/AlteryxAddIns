namespace JDunkerley.AlteryxAddIns.Framework.Interfaces
{
    /// <summary>
    /// Wrap the Alteryx Record Copier
    /// </summary>
    public interface IRecordCopier
    {
        /// <summary>
        /// Copy a RecordData onto a Record
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        void Copy(AlteryxRecordInfoNet.Record destination, AlteryxRecordInfoNet.RecordData source);
    }
}
