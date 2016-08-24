namespace JDunkerley.AlteryxAddIns.Framework.Interfaces
{
    /// <summary>
    /// Represents the method that will handle the <see cref="IInputProperty.RecordPushed"/> event.
    /// </summary>
    /// <param name="sender">The source of the event, typically an <see cref="IInputProperty"/>.</param>
    /// <param name="e">A <see cref="RecordPushedEventArgs"/> that contains the new row of data pushed to the input. The <see cref="SuccessEventArgs.Success"/> property allows the handler to say that the process failed.</param>
    public delegate void RecordPushedEventHandler(object sender, RecordPushedEventArgs e);
}