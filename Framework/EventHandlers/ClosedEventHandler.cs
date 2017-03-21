using OmniBus.Framework.Interfaces;

namespace OmniBus.Framework.EventHandlers
{
    /// <summary>
    ///     Represents the method that will handle the <see cref="IInputProperty.InitCalled" /> event.
    /// </summary>
    /// <param name="sender">The source of the event, typically an <see cref="IInputProperty" />.</param>
    public delegate void ClosedEventHandler(IInputProperty sender);
}