using System;

using AlteryxRecordInfoNet;

namespace OmniBus.Framework.Attributes
{
    /// <summary>
    ///     Specifies associated input field for configuration GUIs to read available fields from.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class InputPropertyNameAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InputPropertyNameAttribute" /> class.
        /// </summary>
        /// <param name="inputConnectionName">Name of the field.</param>
        /// <param name="engineType">Type of the engine</param>
        /// <param name="fieldTypes">List of valid types (defaults to all)</param>
        public InputPropertyNameAttribute(string inputConnectionName, Type engineType, params FieldType[] fieldTypes)
        {
            this.InputConnectionName = inputConnectionName;
            this.EngineType = engineType;

            this.FieldTypes = fieldTypes;
            if (fieldTypes.Length == 0)
            {
                this.FieldTypes = (FieldType[])Enum.GetValues(typeof(FieldType));
            }
        }

        /// <summary>
        ///     Gets the name of the field.
        /// </summary>
        public string InputConnectionName { get; }

        /// <summary>
        ///     Gets the type of the engine.
        /// </summary>
        public Type EngineType { get; }

        /// <summary>
        ///     Gets the field types list.
        /// </summary>
        public FieldType[] FieldTypes { get; }
    }
}