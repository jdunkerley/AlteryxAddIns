using System.Xml;

using AlteryxRecordInfoNet;

namespace OmniBus.Framework.Interfaces
{
    /// <summary>
    ///     Interface Defining An Output Helper
    /// </summary>
    public interface IOutputHelper
    {
        /// <summary>
        ///     Gets a reusable <see cref="Record" /> object
        /// </summary>
        Record Record { get; }

        /// <summary>
        ///     Gets the <see cref="RecordInfo" /> describing the Output records
        /// </summary>
        RecordInfo RecordInfo { get; }

        /// <summary>
        ///     Given a fieldName, gets the <see cref="FieldBase" /> for it
        /// </summary>
        /// <param name="fieldName">Name of field</param>
        /// <returns><see cref="FieldBase" /> representing the field</returns>
        FieldBase this[string fieldName] { get; }

        /// <summary>
        ///     Initializes the output stream.
        /// </summary>
        /// <param name="recordInfo">RecordInfo defining the fields and outputs of the connection.</param>
        /// <param name="sortConfig">Sort configuration to pass onto Alteryx.</param>
        /// <param name="oldConfig">XML configuration of the tool.</param>
        void Init(RecordInfo recordInfo, XmlElement sortConfig = null, XmlElement oldConfig = null);

        /// <summary>
        ///     Pushes a record to Alteryx to hand onto over tools.
        /// </summary>
        /// <param name="record">Record object to push to the stream.</param>
        /// <param name="close">Value indicating whether to close the connection after pushing the record.</param>
        /// <param name="updateCountMod">How often to update Row Count and Data</param>
        void Push(Record record, bool close = false, ulong updateCountMod = 250);

        /// <summary>
        ///     Update The Progress Of A Connection
        /// </summary>
        /// <param name="percentage">Percentage Progress from 0.0 to 1.0</param>
        /// <param name="setToolProgress">Set Tool Progress As Well</param>
        void UpdateProgress(double percentage, bool setToolProgress = false);

        /// <summary>
        ///     Tell Alteryx We Are Finished
        /// </summary>
        /// <param name="executionComplete">Tell Alteryx Tool Execution Is Complete</param>
        void Close(bool executionComplete = false);
    }
}