namespace JDunkerley.AlteryxAddIns.Framework.Interfaces
{
    using System;

    /// <summary>
    /// EventArgs for Progress Updated
    /// </summary>
    public class ProgressUpdatedEventArgs : EventArgs
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
        /// Gets the new progress level
        /// </summary>
        public double Progress { get; }
    }
}