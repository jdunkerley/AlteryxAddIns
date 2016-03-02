namespace JDunkerley.AlteryxAddIns.Framework
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Xml;

    using AlteryxRecordInfoNet;

    using JDunkerley.AlteryxAddIns.Framework.Attributes;

    public class InputFieldTypeConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            InputPropertyNameAttribute attrib = null;
            foreach (var attribute in context.PropertyDescriptor?.Attributes ?? new AttributeCollection())
            {
                attrib = attribute as InputPropertyNameAttribute;
                if (attrib != null)
                {
                    break;
                }
            }

            if (attrib == null)
            {
                return base.GetStandardValues(context);
            }

            var input =
                attrib.EngineType.GetConnections<IIncomingConnectionInterface>()
                    .Select((kvp, i) => new { kvp.Key, i })
                    .FirstOrDefault(o => o.Key == attrib.FieldName);

            if (input == null || InputPropertyNameAttribute.CurrentMetaData == null || InputPropertyNameAttribute.CurrentMetaData.Length <= input.i)
            {
                return base.GetStandardValues(context);
            }

            var sourceElement = InputPropertyNameAttribute.CurrentMetaData[input.i]?.SelectNodes("RecordInfo/Field");
            if (sourceElement == null)
            {
                return base.GetStandardValues(context);
            }

            var names = new List<string>();
            foreach (XmlNode field in sourceElement)
            {
                var name = field.SelectSingleNode("@name")?.Value;
                var fieldTypeName = field.SelectSingleNode("@type")?.Value;
                FieldType fieldType;
                if (name == null || fieldTypeName == null || !Enum.TryParse("E_FT_" + fieldTypeName, out fieldType)
                    || !attrib.FieldTypes.Contains(fieldType))
                {
                    continue;
                }

                names.Add(name);
            }

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
