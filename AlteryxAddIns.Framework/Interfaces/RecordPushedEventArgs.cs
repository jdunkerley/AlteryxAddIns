namespace JDunkerley.AlteryxAddIns.Framework.Interfaces
{
    /// <summary>
    /// <see cref="SuccessEventArgs"/> With A RecordData For When Alteryx Pushes A Record To An Input
    /// </summary>
    public class RecordPushedEventArgs : SuccessEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordPushedEventArgs"/> class.
        /// </summary>
        /// <param name="recordData">Data from Alteryx</param>
        public RecordPushedEventArgs(AlteryxRecordInfoNet.RecordData recordData)
        {
            this.RecordData = recordData;
        }

        /// <summary>
        /// Gets The Pushed Record Data
        /// </summary>
        public AlteryxRecordInfoNet.RecordData RecordData { get; }
    }
}