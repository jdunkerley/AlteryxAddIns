using System;
using System.Globalization;

using AlteryxRecordInfoNet;

namespace OmniBus.Framework
{
    /// <summary>
    ///     Extension methods for working with the <see cref="FieldBase" /> class.
    /// </summary>
    public static class FieldBaseHelpers
    {
        private static readonly string[] AlteryxDateFormats = { "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss", "HH:mm:ss" };

        /// <summary>
        ///     Read field value from a <see cref="RecordData" /> and then parse to a <see cref="DateTime" />.
        /// </summary>
        /// <param name="field">FieldBase describing the field.</param>
        /// <param name="recordData">Row's record data to read.</param>
        /// <returns>Parsed DateTime or NULL.</returns>
        public static DateTime? GetAsDateTime(this FieldBase field, RecordData recordData)
        {
            var text = field.GetAsString(recordData);
            return !string.IsNullOrWhiteSpace(text)
                   && DateTime.TryParseExact(
                       text,
                       AlteryxDateFormats,
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.AllowWhiteSpaces,
                       out DateTime output)
                       ? (DateTime?)output
                       : null;
        }

        /// <summary>
        ///     Read field value from a <see cref="RecordData" /> and then parse to a <see cref="TimeSpan" />.
        /// </summary>
        /// <param name="field">FieldBase describing the field.</param>
        /// <param name="recordData">Row's record data to read.</param>
        /// <returns>Parsed TimeSpan or NULL.</returns>
        public static TimeSpan? GetAsTimeSpan(this FieldBase field, RecordData recordData)
        {
            var dateTime = field.GetAsDateTime(recordData);
            return dateTime?.TimeOfDay;
        }
    }
}