namespace OmniBus.Framework.EventHandlers
{
    /// <summary>
    ///     Provides an object with a <see cref="Success" /> property to allow handlers to inform sender if handling failed.
    /// </summary>
    public class SuccessEventArgs : System.EventArgs
    {
        /// <summary>
        ///     Gets or sets a value indicating whether successful or not
        /// </summary>
        public bool Success { get; private set; } = true;

        /// <summary>
        /// Set the success flag to failed.
        /// </summary>
        public void SetFailed()
        {
            this.Success = false;
        }
    }
}