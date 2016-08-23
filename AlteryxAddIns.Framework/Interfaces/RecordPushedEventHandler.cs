namespace JDunkerley.AlteryxAddIns.Framework.Interfaces
{
    /// <summary>
    /// Represents the method that will handle an event that occurs when Alteryx pushes a record with .
    /// An event handler for <see cref="RecordPushedEventArgs"/> based events
    /// </summary>
    /// <param name="sender">The Sender Input Property</param>
    /// <param name="e">The </param>
    public delegate void RecordPushedEventHandler(object sender, RecordPushedEventArgs e);
}