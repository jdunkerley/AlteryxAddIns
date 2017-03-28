using System;
using System.Linq;

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
        public FieldDescription(string name, FieldType fieldType, int size = 0, int scale = 0, string source = null)
        {
            this.Name = name;
            this.FieldType = fieldType;
            this.Size = size;
            this.Scale = scale;
            this.Source = (source?.EndsWith("Engine") ?? false) ? source.Substring(0, source.Length - 6) : source;
        }

        /// <summary>
        /// Gets the Maximum String Length
        /// </summary>
        public static int MaxStringLength { get; } = 1_073_741_823;

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
        public int Size { get; }

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

        /// <summary>
        ///     Given a set of <see cref="FieldDescription" />, create a new RecordInfo with specified fields..
        /// </summary>
        /// <param name="fields">Set of Field Descriptions.</param>
        /// <returns>A configured RecordInfo object.</returns>
        public static RecordInfo CreateRecordInfo(params FieldDescription[] fields)
        {
            return CreateRecordInfo(null, fields);
        }

        /// <summary>
        ///     Given a set of <see cref="FieldDescription" /> and existing RecordInfo, create a new RecordInfo with specified
        ///     fields replacing existing ones or being appended.
        /// </summary>
        /// <param name="inputRecordInfo">Existing RecordInfo object.</param>
        /// <param name="fields">Set of Field Descriptions.</param>
        /// <returns>A configured RecordInfo object.</returns>
        public static RecordInfo CreateRecordInfo(RecordInfo inputRecordInfo, params FieldDescription[] fields)
        {
            var fieldDict = fields.ToDictionary(f => f.Name, f => false);
            var output = new RecordInfo();

            if (inputRecordInfo != null)
            {
                for (var i = 0; i < inputRecordInfo.NumFields(); i++)
                {
                    var fieldInfo = inputRecordInfo[i];
                    var fieldName = fieldInfo.GetFieldName();

                    if (fieldDict.ContainsKey(fieldName))
                    {
                        fieldDict[fieldName] = true;
                        var descr = fields.First(f => f.Name == fieldName);
                        output.AddField(
                            descr.Name,
                            descr.FieldType,
                            descr.Size,
                            descr.Scale,
                            descr.Source,
                            descr.Description);
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