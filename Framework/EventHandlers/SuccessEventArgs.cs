namespace OmniBus.Framework.EventHandlers
{
    /// <summary>
    ///     Provides the <see cref="OmniBus.Framework.Interfaces.IInputProperty.InitCalled" /> event with a <see cref="Success" /> property to inform input
    ///     propery if handling failed.
    /// </summary>
    public class SuccessEventArgs : System.EventArgs
    {
        /// <summary>
        ///     Gets or sets a value indicating whether successful or not
        /// </summary>
        public bool Success { get; set; } = true;
    }
}