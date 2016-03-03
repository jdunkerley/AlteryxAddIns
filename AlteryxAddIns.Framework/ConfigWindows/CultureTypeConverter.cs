namespace JDunkerley.AlteryxAddIns.Framework.ConfigWindows
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;

    public class CultureTypeConverter : TypeConverter
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

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            return value as string ?? base.ConvertFrom(context, culture, value);
        }
    }
}

