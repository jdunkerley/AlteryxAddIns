namespace JDunkerley.AlteryxAddIns.Framework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;


    /// <summary>
    /// Shared Static Properties
    /// </summary>
    public static class Statics
    {

        /// <summary>
        /// Gets or sets the current meta data.
        /// </summary>
        public static XmlElement[] CurrentMetaData { get; set; }

        /// <summary>
        /// Given Current MetaData Get Matching Fields
        /// </summary>
        /// <param name="engineType"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldTypes"></param>
        /// <returns></returns>
        public static ICollection GetFieldList(Type engineType, string fieldName, IList<AlteryxRecordInfoNet.FieldType> fieldTypes)
        {
            var input = engineType.GetConnections<AlteryxRecordInfoNet.IIncomingConnectionInterface>()
                    .Select((kvp, i) => new { kvp.Key, i })
                    .FirstOrDefault(o => o.Key == fieldName);

            if (input == null || CurrentMetaData == null || CurrentMetaData.Length <= input.i)
            {
                return null;
            }

            var sourceElement = CurrentMetaData[input.i]?.SelectNodes("RecordInfo/Field");
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
