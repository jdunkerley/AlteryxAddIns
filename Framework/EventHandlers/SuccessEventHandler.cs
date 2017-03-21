using OmniBus.Framework.Interfaces;

namespace OmniBus.Framework.EventHandlers
{
    /// <summary>
    ///     Represents the method that will handle the <see cref="IInputProperty.InitCalled" /> event.
    /// </summary>
    /// <param name="sender">The source of the event, typically an <see cref="IInputProperty" />.</param>
    /// <param name="e">
    ///     A <see cref="SuccessEventArgs" /> object whose<see cref="SuccessEventArgs.Success" /> property allows
    ///     the handler to say that the process failed.
    /// </param>
    public delegate void SuccessEventHandler(IInputProperty sender, SuccessEventArgs e);
}