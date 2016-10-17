namespace JDunkerley.AlteryxAddIns.Framework
{
    using System;
    using System.Linq;

    using AlteryxRecordInfoNet;

    /// <summary>
    /// Simple Field Descriptor Class
    /// </summary>
    public class FieldDescription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldDescription"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="fieldType">Type of the field.</param>
        public FieldDescription(string name, FieldType fieldType)
        {
            this.Name = name;
            this.FieldType = fieldType;
        }

        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        public FieldType FieldType { get; }

        /// <summary>
        /// Gets or sets the size of the field in bytes.
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the scale of the field.
        /// </summary>
        public int Scale { get; set; }

        /// <summary>
        /// Gets or sets the source of the field.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the description of the field.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Given a .Net Type and a name create a FieldDescription
        /// </summary>
        /// <param name="name">Field Name</param>
        /// <param name="type">DotNet Type</param>
        /// <returns>FieldDescription for Alteryx</returns>
        public static FieldDescription FromNameAndType(string name, Type type)
        {
            switch (type.Name)
            {
                case nameof(String):
                    return new FieldDescription(name, FieldType.E_FT_V_WString) { Size = 32000 };

                case nameof(DateTime):
                    return new FieldDescription(name, FieldType.E_FT_DateTime);

                case nameof(Boolean):
                    return new FieldDescription(name, FieldType.E_FT_Bool);

                case nameof(Byte):
                    return new FieldDescription(name, FieldType.E_FT_Byte);

                case nameof(Int16):
                    return new FieldDescription(name, FieldType.E_FT_Int16);

                case nameof(Int32):
                    return new FieldDescription(name, FieldType.E_FT_Int32);

                case nameof(Int64):
                    return new FieldDescription(name, FieldType.E_FT_Int64);

                case nameof(Double):
                    return new FieldDescription(name, FieldType.E_FT_Double);

                case nameof(Single):
                    return new FieldDescription(name, FieldType.E_FT_Float);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Given a set of <see cref="FieldDescription"/>, create a new RecordInfo with specified fields..
        /// </summary>
        /// <param name="fields">Set of Field Descriptions.</param>
        /// <returns>A configured RecordInfo object.</returns>
        public static RecordInfo CreateRecordInfo(
            params FieldDescription[] fields)
        {
            return CreateRecordInfo(null, fields);
        }

        /// <summary>
        /// Given a set of <see cref="FieldDescription"/> and existing RecordInfo, create a new RecordInfo with specified fields replacing existing ones or being appended.
        /// </summary>
        /// <param name="inputRecordInfo">Existing RecordInfo object.</param>
        /// <param name="fields">Set of Field Descriptions.</param>
        /// <returns>A configured RecordInfo object.</returns>
        public static RecordInfo CreateRecordInfo(
            RecordInfo inputRecordInfo,
            params FieldDescription[] fields)
        {
            var fieldDict = fields.ToDictionary(f => f.Name, f => false);
            var output = new RecordInfo();

            if (inputRecordInfo != null)
            {
                for (int i = 0; i < inputRecordInfo.NumFields(); i++)
                {
                    var fieldInfo = inputRecordInfo[i];
                    var fieldName = fieldInfo.GetFieldName();

                    if (fieldDict.ContainsKey(fieldName))
                    {
                        fieldDict[fieldName] = true;
                        var descr = fields.First(f => f.Name == fieldName);
                        output.AddField(descr.Name, descr.FieldType, descr.Size, descr.Scale, descr.Source, descr.Description);
                        continue;
                    }

                    output.AddField(fieldInfo);
                }
            }

            foreach (var descr in fields.Where(d => !fieldDict[d.Name]))
            {
                output.AddField(descr.Name, descr.FieldType, descr.Size, descr.Scale, descr.Source, descr.Description);
            }

            return output;
        }
    }
}