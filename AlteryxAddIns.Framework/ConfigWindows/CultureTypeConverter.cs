namespace JDunkerley.AlteryxAddIns.Framework.ConfigWindows
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;

    public class CultureTypeConverter : StringConverter
    {
        public const string Current = "Current";

        public const string Invariant = "Invariant";

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var strings = new List<string> { Current, Invariant };
            strings.AddRange(CultureInfo.GetCultures(CultureTypes.AllCultures).Select(c => c.DisplayName).Distinct());
            return new StandardValuesCollection(strings);
        }

        public static CultureInfo GetCulture(string name)
        {
            if (name == Current)
            {
                return CultureInfo.CurrentCulture;
            }
            if (name == Invariant)
            {
                return CultureInfo.InvariantCulture;
            }
            return CultureInfo.GetCultures(CultureTypes.AllCultures).First(c => c.DisplayName == name);
        }
    }
}

