namespace JDunkerley.AlteryxAddIns.Framework
{
    // ToDo: Add support for fixed decimal.

    /// <summary>
    /// Enumeration Representing All The Alteryx DataTypes
    /// </summary>
    public enum OutputType
    {
        /// <summary>
        /// Boolean
        /// </summary>
        Bool = AlteryxRecordInfoNet.FieldType.E_FT_Bool,

        /// <summary>
        /// Byte Value
        /// </summary>
        Byte = AlteryxRecordInfoNet.FieldType.E_FT_Byte,

        /// <summary>
        /// Int16 Value
        /// </summary>
        Int16 = AlteryxRecordInfoNet.FieldType.E_FT_Int16,

        /// <summary>
        /// Int32 Value
        /// </summary>
        Int32 = AlteryxRecordInfoNet.FieldType.E_FT_Int32,

        /// <summary>
        /// Int64 Value
        /// </summary>
        Int64 = AlteryxRecordInfoNet.FieldType.E_FT_Int64,

        /// <summary>
        /// Float Value
        /// </summary>
        Float = AlteryxRecordInfoNet.FieldType.E_FT_Float,

        /// <summary>
        /// Double Value
        /// </summary>
        Double = AlteryxRecordInfoNet.FieldType.E_FT_Double,

        /// <summary>
        /// Date Value
        /// </summary>
        Date = AlteryxRecordInfoNet.FieldType.E_FT_Date,

        /// <summary>
        /// DateTime Value
        /// </summary>
        DateTime = AlteryxRecordInfoNet.FieldType.E_FT_DateTime,

        /// <summary>
        /// Time Value
        /// </summary>
        Time = AlteryxRecordInfoNet.FieldType.E_FT_Time,

        /// <summary>
        /// Fixed Length String
        /// </summary>
        String = AlteryxRecordInfoNet.FieldType.E_FT_String,

        /// <summary>
        /// Variable Length String
        /// </summary>
        VString = AlteryxRecordInfoNet.FieldType.E_FT_V_String,

        /// <summary>
        /// Fixed Length Unicode String
        /// </summary>
        WString = AlteryxRecordInfoNet.FieldType.E_FT_WString,

        /// <summary>
        /// Variable Length Unicode String
        /// </summary>
        // ReSharper disable once InconsistentNaming
        VWString = AlteryxRecordInfoNet.FieldType.E_FT_V_WString
    }
}
