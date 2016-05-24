namespace JDunkerley.AlteryxAddIns.Framework.ConfigWindows
{
    using System.ComponentModel;

    using Attributes;

    public class InputFieldTypeConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;

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
    }
}
