namespace JDunkerley.AlteryxAddIns.Framework.Interfaces
{
    /// <summary>
    /// Provides data for the <see cref="IInputProperty.RecordPushed"/> event with a <see cref="SuccessEventArgs.Success"/> property to inform input property if handling failed.
    /// </summary>
    public class RecordPushedEventArgs : SuccessEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordPushedEventArgs"/> class.
        /// </summary>
        /// <param name="recordData">Record data from Alteryx</param>
        public RecordPushedEventArgs(AlteryxRecordInfoNet.RecordData recordData)
        {
            this.RecordData = recordData;
        }

        /// <summary>
        /// Gets the pushed record data
        /// </summary>
        public AlteryxRecordInfoNet.RecordData RecordData { get; }
    }
}