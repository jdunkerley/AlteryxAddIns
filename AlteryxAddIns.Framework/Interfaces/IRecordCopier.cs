namespace JDunkerley.AlteryxAddIns.Framework.Interfaces
{
    /// <summary>
    /// Wrap the Alteryx Record Copier
    /// </summary>
    public interface IRecordCopier
    {
        /// <summary>
        /// Copy a <see cref="AlteryxRecordInfoNet.RecordData"/> onto a <see cref="AlteryxRecordInfoNet.Record"/>.
        /// </summary>
        /// <param name="destination">The destination record object to copy to.</param>
        /// <param name="source">The source record data object from Alteryx.</param>
        void Copy(AlteryxRecordInfoNet.Record destination, AlteryxRecordInfoNet.RecordData source);
    }
}
