namespace JDunkerley.AlteryxAddIns.Framework.Interfaces
{
    /// <summary>
    /// Represents the method that will handle the <see cref="IInputProperty.ProgressUpdated"/> event.
    /// </summary>
    /// <param name="sender">The source of the event, typically an <see cref="IInputProperty"/>.</param>
    /// <param name="e">A <see cref="ProgressUpdatedEventHandler"/> that contains the updated progress percentage.</param>
    public delegate void ProgressUpdatedEventHandler(object sender, ProgressUpdatedEventArgs e);
}