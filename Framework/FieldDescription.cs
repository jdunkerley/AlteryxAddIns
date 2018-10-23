using System;

using AlteryxRecordInfoNet;

namespace OmniBus.Framework
{
    /// <summary>
    ///     Simple Field Descriptor Class
    /// </summary>
    public class FieldDescription
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FieldDescription" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="fieldType">Type of the field.</param>
        /// <param name="size">Size of the field in bytes.</param>
        /// <param name="scale">Scale of the field.</param>
        /// <param name="source">Source of the field.</param>
        /// <param name="description">Description of the field.</param>
        public FieldDescription(string name, FieldType fieldType, uint size = 0, int scale = 0, string source = null, string description = null)
        {
            this.Name = name;
            this.FieldType = fieldType;
            this.Size = size;
            this.Scale = scale;
            this.Source = (source?.EndsWith("Engine") ?? false) ? source.Substring(0, source.Length - 6) : source;
            this.Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldDescription"/> class from an <see cref="FieldBase"/>.
        /// </summary>
        /// <param name="f">Field base to copy from</param>
        public FieldDescription(FieldBase f)
            : this(f.GetFieldName(), f.FieldType, f.Size, f.Scale, f.GetSource(), f.GetDescription())
        {
        }

        /// <summary>
        /// Gets the Maximum String Length
        /// </summary>
        public static uint MaxStringLength { get; } = 1_073_741_824;

        /// <summary>
        ///     Gets the name of the field.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the type of the field.
        /// </summary>
        public FieldType FieldType { get; }

        /// <summary>
        ///     Gets the size of the field in bytes.
        /// </summary>
        public uint Size { get; }

        /// <summary>
        ///     Gets the scale of the field.
        /// </summary>
        public int Scale { get; }

        /// <summary>
        ///     Gets the source of the field.
        /// </summary>
        public string Source { get; }

        /// <summary>
        ///     Gets or sets the description of the field.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets appropriate size for the field
        /// </summary>
        internal uint ParsedSize
        {
            get
            {
                switch (this.FieldType)
                {
                    case FieldType.E_FT_Bool:
                    case FieldType.E_FT_Byte: return 1;
                    case FieldType.E_FT_Int16: return 2;
                    case FieldType.E_FT_Int32:
                    case FieldType.E_FT_Float: return 4;
                    case FieldType.E_FT_Int64:
                    case FieldType.E_FT_Double: return 8;
                    case FieldType.E_FT_Date: return 10;
                    case FieldType.E_FT_Time: return 8;
                    case FieldType.E_FT_DateTime: return 19;
                    case FieldType.E_FT_V_String:
                    case FieldType.E_FT_V_WString: return this.Size == 0 ? MaxStringLength : this.Size;
                }

                return this.Size;
            }
        }

        /// <summary>
        ///     Given a .Net Type and a name create a FieldDescription
        /// </summary>
        /// <param name="type">DotNet Type</param>
        /// <returns>FieldDescription for Alteryx</returns>
        public static FieldType GetAlteryxFieldType(Type type)
        {
            switch (type.Name)
            {
                case nameof(String): return FieldType.E_FT_V_WString;
                case nameof(DateTime): return FieldType.E_FT_DateTime;
                case nameof(Boolean): return FieldType.E_FT_Bool;
                case nameof(Byte): return FieldType.E_FT_Byte;
                case nameof(Int16): return FieldType.E_FT_Int16;
                case nameof(Int32): return FieldType.E_FT_Int32;
                case nameof(Int64): return FieldType.E_FT_Int64;
                case nameof(Double): return FieldType.E_FT_Double;
                case nameof(Single): return FieldType.E_FT_Float;
                default: return FieldType.E_FT_Unknown;
            }
        }
    }
}