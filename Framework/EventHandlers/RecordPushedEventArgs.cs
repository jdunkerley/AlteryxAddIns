namespace OmniBus.Framework.EventHandlers
{
    /// <summary>
    ///     Provides data for the <see cref="OmniBus.Framework.Interfaces.IInputProperty.RecordPushed" /> event with a
    ///     <see cref="SuccessEventArgs.Success" /> property to inform input property if handling failed.
    /// </summary>
    public class RecordPushedEventArgs : SuccessEventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RecordPushedEventArgs" /> class.
        /// </summary>
        /// <param name="recordData">Record data from Alteryx</param>
        public RecordPushedEventArgs(AlteryxRecordInfoNet.RecordData recordData)
        {
            this.RecordData = recordData;
        }

        /// <summary>
        ///     Gets the pushed record data
        /// </summary>
        public AlteryxRecordInfoNet.RecordData RecordData { get; }
    }
}