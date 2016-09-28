namespace JDunkerley.AlteryxAddIns.Framework.Interfaces
{
    /// <summary>
    /// Provides data for the <see cref="IInputProperty.ProgressUpdated"/> event
    /// </summary>
    public class ProgressUpdatedEventArgs : System.EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressUpdatedEventArgs"/> class.
        /// </summary>
        /// <param name="progress">Progress Percentage (0 to 1)</param>
        public ProgressUpdatedEventArgs(double progress)
        {
            this.Progress = progress;
        }

        /// <summary>
        /// Gets the inputs progress percentage.
        /// </summary>
        public double Progress { get; }
    }
}