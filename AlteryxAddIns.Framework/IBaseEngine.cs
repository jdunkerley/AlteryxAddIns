namespace JDunkerley.AlteryxAddIns.Framework
{
    /// <summary>
    /// Interface To Get Engine and Tool Id
    /// </summary>
    public interface IBaseEngine
    {
        /// <summary>
        /// Gets the Alteryx engine.
        /// </summary>
        AlteryxRecordInfoNet.EngineInterface Engine { get; }

        /// <summary>
        /// Gets the tool identifier. Set at PI_Init, unset at PI_Close.
        /// </summary>
        int NToolId { get; }

        /// <summary>
        /// Gets the XML configuration from the work flow.
        /// </summary>
        System.Xml.XmlElement XmlConfig { get; }

        /// <summary>
        /// Tell Alteryx execution is complete.
        /// </summary>
        void ExecutionComplete();

        /// <summary>
        /// Sends a Debug message to the Alteryx log window.
        /// </summary>
        /// <param name="message">Message text.</param>
        void DebugMessage(string message);
    }
}