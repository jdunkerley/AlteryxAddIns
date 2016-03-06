namespace JDunkerley.AlteryxAddIns.Framework.ConfigWindows
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    using JDunkerley.AlteryxAddIns.Framework.Attributes;

    public class FixedListTypeConverter<T> : TypeConverter
    {
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var dict = context.PropertyDescriptor?.Attributes.GetAttrib<FieldListAttribute>()?.DictionaryLookUp;
            string valueString = value as string;
            if (dict != null && valueString != null)
            {
                object output;
                if (dict.TryGetValue(valueString, out output))
                {
                    return (T)output;
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return context.PropertyDescriptor?.Attributes.GetAttrib<FieldListAttribute>()?.StandardValuesCollection
                   ?? base.GetStandardValues(context);
        }
    }
}
