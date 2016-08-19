namespace JDunkerley.AlteryxAddIns.Framework.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// For the designers where the property has a fixed list of values
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FieldListAttribute : Attribute
    {
        public FieldListAttribute(params object[] values)
        {
            this.DictionaryLookUp = values.ToDictionary(v => v.ToString(), v => v);
            this.StandardValuesCollection = new TypeConverter.StandardValuesCollection(values.Select(v => v.ToString()).ToList());
        }

        public Dictionary<string, object> DictionaryLookUp { get; }

        public TypeConverter.StandardValuesCollection StandardValuesCollection { get; }

        public IEnumerable<KeyValuePair<string, object>> OrderedDictionary
            =>
                this.StandardValuesCollection.Cast<string>()
                    .Select(k => new KeyValuePair<string, object>(k, this.DictionaryLookUp[k]));
    }
}