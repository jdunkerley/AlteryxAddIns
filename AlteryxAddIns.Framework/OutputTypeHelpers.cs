namespace JDunkerley.AlteryxAddIns.Framework
{
    /// <summary>
    /// Extension methods for <see cref="OutputType"/> enumeration.
    /// </summary>
    public static class OutputTypeHelpers
    {
        /// <summary>
        /// Create a <see cref="FieldDescription "/> object given an <see cref="OutputType"/>, field name and optionally a size
        /// </summary>
        /// <param name="outputType">The field type needed.</param>
        /// <param name="fieldName">The name for the field.</param>
        /// <param name="size">The size of the field in byte for variable length types.</param>
        /// <returns>Correctly configured FieldDescription.</returns>
        public static FieldDescription OutputDescription(this OutputType outputType, string fieldName, int size = 0)
        {
            switch (outputType)
            {
                case OutputType.Bool:
                case OutputType.Byte:
                case OutputType.Int16:
                case OutputType.Int32:
                case OutputType.Int64:
                case OutputType.Float:
                case OutputType.Double:
                case OutputType.Date:
                case OutputType.DateTime:
                case OutputType.Time:
                    return new FieldDescription(fieldName, (AlteryxRecordInfoNet.FieldType)outputType);
                case OutputType.String:
                case OutputType.VString:
                case OutputType.WString:
                case OutputType.VWString:
                    return new FieldDescription(fieldName, (AlteryxRecordInfoNet.FieldType)outputType) { Size = size };
            }

            return null;
        }
    }
}