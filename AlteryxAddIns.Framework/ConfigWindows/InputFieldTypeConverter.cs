namespace JDunkerley.AlteryxAddIns.Framework.ConfigWindows
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Xml;

    using Attributes;

    /// <summary>
    /// Provides a type converter to allow a user to select from a set of incoming fields.
    /// Uses an <see cref="InputPropertyNameAttribute"/> to get list of fields and allows for type filtering.
    /// </summary>
    public class InputFieldTypeConverter : StringConverter
    {
        /// <summary>Returns whether this object supports a standard set of values that can be picked from a list, using the specified context.</summary>
        /// <returns>true if <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> should be called to find a common set of values the object supports; otherwise, false.</returns>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;

        /// <summary>Returns a collection of standard values for the data type this type converter is designed for when provided with a format context.</summary>
        /// <returns>A <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection" /> that holds a standard set of valid values, or null if the data type does not support a standard set of values.</returns>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context that can be used to extract additional information about the environment from which this converter is invoked. This parameter or properties of this parameter can be null. </param>
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

            var instance = context.Instance as ConfigWithIncomingConnection;
            if (instance == null)
            {
                return base.GetStandardValues(context);
            }

            var names = GetFieldList(instance.IncomingMetaInfo, attrib.EngineType, attrib.InputConnectionName, attrib.FieldTypes);
            return new StandardValuesCollection(names);
        }

        private static ICollection GetFieldList(XmlElement[] incomingMetaData, Type engineType, string connectionName, IList<AlteryxRecordInfoNet.FieldType> fieldTypes)
        {
            var input = engineType.GetProperties<AlteryxRecordInfoNet.IIncomingConnectionInterface>()
                    .Select((kvp, i) => new { kvp.Key, i })
                    .FirstOrDefault(o => o.Key == connectionName);

            if (input == null || incomingMetaData == null || incomingMetaData.Length <= input.i)
            {
                return null;
            }

            var sourceElement = incomingMetaData[input.i]?.SelectNodes("RecordInfo/Field");
            if (sourceElement == null)
            {
                return null;
            }

            var names = new List<string>();
            foreach (XmlNode field in sourceElement)
            {
                var name = field.SelectSingleNode("@name")?.Value;
                var fieldTypeName = field.SelectSingleNode("@type")?.Value;
                AlteryxRecordInfoNet.FieldType fieldType;
                if (name == null || fieldTypeName == null || !Enum.TryParse("E_FT_" + fieldTypeName, out fieldType)
                    || !fieldTypes.Contains(fieldType))
                {
                    continue;
                }

                names.Add(name);
            }

            return names;
        }
    }
}
