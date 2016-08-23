namespace JDunkerley.AlteryxAddIns.Framework.ConfigWindows
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;

    public class CultureTypeConverter : StringConverter
    {
        public const string Current = "Current";

        public const string Invariant = "Invariant";

        /// <summary>
        /// Given a culture name, gets the <see cref="CultureInfo"/> for it
        /// </summary>
        /// <param name="name">Name of Culture</param>
        /// <returns><see cref="CultureInfo"/> or NULL if not found</returns>
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

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var strings = new List<string> { Current, Invariant };
            strings.AddRange(CultureInfo.GetCultures(CultureTypes.AllCultures).Select(c => c.DisplayName).Distinct());
            return new StandardValuesCollection(strings);
        }
    }
}