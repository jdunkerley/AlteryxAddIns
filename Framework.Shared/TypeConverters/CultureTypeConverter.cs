using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace OmniBus.Framework.TypeConverters
{
    /// <summary>
    ///     Provides a type converter to convert CultureInfo objects to and from string representations.
    /// </summary>
    public class CultureTypeConverter : StringConverter
    {
        /// <summary>
        ///     The Current Machine Culture
        /// </summary>
        public const string Current = "Current";

        /// <summary>
        ///     The Invariant Culture
        /// </summary>
        public const string Invariant = "Invariant";

        /// <summary>
        ///     Given a culture name, gets the <see cref="CultureInfo" /> for it
        /// </summary>
        /// <param name="name">Name of Culture</param>
        /// <returns><see cref="CultureInfo" /> or NULL if not found</returns>
        public static CultureInfo GetCulture(string name)
        {
            switch (name)
            {
                case Current:
                    return CultureInfo.CurrentCulture;
                case Invariant:
                    return CultureInfo.InvariantCulture;
                default:
                    return CultureInfo.GetCultures(CultureTypes.AllCultures).FirstOrDefault(c => c.DisplayName == name);
            }
        }

        /// <summary>
        ///     Returns whether this object supports a standard set of values that can be picked from a list, using the
        ///     specified context.
        /// </summary>
        /// <returns>
        ///     true if <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> should be called to find a
        ///     common set of values the object supports; otherwise, false.
        /// </returns>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;

        /// <summary>
        ///     Returns a collection of standard values for the data type this type converter is designed for when provided
        ///     with a format context.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection" /> that holds a standard set of
        ///     valid values, or null if the data type does not support a standard set of values.
        /// </returns>
        /// <param name="context">
        ///     An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context
        ///     that can be used to extract additional information about the environment from which this converter is invoked. This
        ///     parameter or properties of this parameter can be null.
        /// </param>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var strings = new List<string> { Current, Invariant };
            strings.AddRange(CultureInfo.GetCultures(CultureTypes.AllCultures).Select(c => c.DisplayName).Distinct());
            return new StandardValuesCollection(strings);
        }
    }
}