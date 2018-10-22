namespace OmniBus.Framework.Interfaces
{
    /// <summary>
    /// Interface Controlling Showing Debug Messages
    /// </summary>
    public interface IShowDebugMessages
    {
        /// <summary>
        ///     Tells Alteryx whether to show debug error messages or not.
        /// </summary>
        /// <returns>A value indicating whether to show debug error messages or not.</returns>
        bool ShowDebugMessages();
    }
}