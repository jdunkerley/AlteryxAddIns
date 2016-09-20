namespace JDunkerley.AlteryxAddIns.Framework
{
    using System;

    using AlteryxRecordInfoNet;

    /// <summary>
    /// Extension methods for working with the <see cref="FieldBase"/> class.
    /// </summary>
    public static class FieldBaseHelpers
    {
        /// <summary>
        /// Read field value from a <see cref="RecordData"/> and then parse to a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="field">FieldBase describing the field.</param>
        /// <param name="recordData">Row's record data to read.</param>
        /// <returns>Parsed DateTime or NULL.</returns>
        public static DateTime? GetAsDateTime(this FieldBase field, RecordData recordData)
        {
            var text = field.GetAsString(recordData);
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            DateTime output;
            if (!DateTime.TryParse(text, out output))
            {
                return null;
            }

            return output;
        }

        /// <summary>
        /// Read field value from a <see cref="RecordData"/> and then parse to a <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="field">FieldBase describing the field.</param>
        /// <param name="recordData">Row's record data to read.</param>
        /// <returns>Parsed TimeSpan or NULL.</returns>
        public static TimeSpan? GetAsTimeSpan(this FieldBase field, RecordData recordData)
        {
            var text = field.GetAsString(recordData);
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            DateTime output;
            if (!DateTime.TryParse($"1900-01-01 {text}", out output))
            {
                return null;
            }

            return output.TimeOfDay;
        }
    }
}