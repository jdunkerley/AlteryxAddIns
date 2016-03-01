namespace JDunkerley.Alteryx.Attributes
{
    using System;
    using System.Xml;

    /// <summary>
    /// Specifies associated input field for configs
    /// </summary>
    public class InputPropertyNameAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputPropertyNameAttribute"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="engineType">Type of the engine</param>
        /// <param name="fieldTypes">List of valid types (defaults to all)</param>
        public InputPropertyNameAttribute(string fieldName, Type engineType, params AlteryxRecordInfoNet.FieldType[] fieldTypes)
        {
            this.FieldName = fieldName;
            this.EngineType = engineType;

            this.FieldTypes = fieldTypes;
            if (fieldTypes.Length == 0)
            {
                this.FieldTypes = (AlteryxRecordInfoNet.FieldType[])Enum.GetValues(typeof(AlteryxRecordInfoNet.FieldType));
            }
        }

        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// Gets the type of the engine.
        /// </summary>
        public Type EngineType { get; }

        /// <summary>
        /// Gets the field types list.
        /// </summary>
        public AlteryxRecordInfoNet.FieldType[] FieldTypes { get; }

        /// <summary>
        /// Gets or sets the current meta data.
        /// </summary>
        public static XmlElement[] CurrentMetaData { get; set; }
    }
}
