namespace JDunkerley.AlteryxAddIns.Framework.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    public class FieldListAttribute : Attribute
    {
        public FieldListAttribute(params object[] values)
        {
            this.DictionaryLookUp = values.ToDictionary(v => v.ToString(), v => v);
            this.StandardValuesCollection = new TypeConverter.StandardValuesCollection(values.Select(v => v.ToString()).ToList());
        }

        public Dictionary<string, object> DictionaryLookUp { get; }

        public TypeConverter.StandardValuesCollection StandardValuesCollection { get; }
    }
}