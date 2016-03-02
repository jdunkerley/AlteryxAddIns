namespace JDunkerley.AlteryxAddIns.Framework
{
    using AlteryxRecordInfoNet;

    /// <summary>
    /// Simple Descriptor Class
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
    }
}