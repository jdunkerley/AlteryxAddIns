namespace JDunkerley.AlteryxAddIns.Framework.Interfaces
{
    using System;

    /// <summary>
    /// <see cref="EventArgs"/> with a flag to indicate success. Defaults to success.
    /// </summary>
    public class SuccessEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether successful or not
        /// </summary>
        public bool Success { get; set; } = true;
    }
}