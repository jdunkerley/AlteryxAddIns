namespace JDunkerley.AlteryxAddIns.Framework.Interfaces
{
    using System;

    /// <summary>
    /// Incoming Connection Engine Interface
    /// </summary>
    public interface IInputProperty : AlteryxRecordInfoNet.IIncomingConnectionInterface
    {
        /// <summary>
        /// Event When Alteryx Calls <see cref="AlteryxRecordInfoNet.IIncomingConnectionInterface.II_Init"/>
        /// </summary>
        event SuccessEventHandler InitCalled;

        /// <summary>
        /// Event When Alteryx Pushes A Record On The Input
        /// </summary>
        event RecordPushedEventHandler RecordPushed;

        /// <summary>
        /// Event to update progress
        /// </summary>
        event ProgressUpdatedEventHandler ProgressUpdated;

        /// <summary>
        /// Event when Alteryx closes the connection
        /// </summary>
        event EventHandler Closed;

        /// <summary>
        /// Gets the current state.
        /// </summary>
        ConnectionState State { get; }

        /// <summary>
        /// Gets the record information of incoming stream.
        /// </summary>
        AlteryxRecordInfoNet.RecordInfo RecordInfo { get; }

        /// <summary>
        /// Gets the record copier for this property.
        /// </summary>
        IRecordCopier Copier { get; }
    }
}