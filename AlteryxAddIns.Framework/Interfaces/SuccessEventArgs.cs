namespace JDunkerley.AlteryxAddIns.Framework.Interfaces
{
    /// <summary>
    /// Provides the <see cref="IInputProperty.InitCalled"/> event with a <see cref="Success"/> property to inform input propery if handling failed.
    /// </summary>
    public class SuccessEventArgs : System.EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether successful or not
        /// </summary>
        public bool Success { get; set; } = true;
    }
}