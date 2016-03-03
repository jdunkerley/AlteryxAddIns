namespace JDunkerley.AlteryxAddIns.Framework.ConfigWindows
{
    using System;
    using System.ComponentModel;

    using JDunkerley.AlteryxAddIns.Framework.Attributes;

    public class InputFieldTypeConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context.PropertyDescriptor == null)
            {
                return base.GetStandardValues(context);
            }

            var attrib = context.PropertyDescriptor.Attributes.GetAttrib<InputPropertyNameAttribute>();
            if (attrib == null)
            {
                return base.GetStandardValues(context);
            }

            var names = Statics.GetFieldList(attrib.EngineType, attrib.FieldName, attrib.FieldTypes);
            return new StandardValuesCollection(names);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            return value as string ?? base.ConvertFrom(context, culture, value);
        }
    }
}
