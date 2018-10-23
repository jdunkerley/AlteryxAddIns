using System.Collections.Generic;
using System.Linq;

using AlteryxRecordInfoNet;

namespace OmniBus.Framework.Builders
{
    /// <summary>
    /// Builder class for making RecordInfos
    /// </summary>
    public class RecordInfoBuilder
    {
        private readonly FieldDescription[] _fieldsDescriptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordInfoBuilder"/> class.
        /// </summary>
        public RecordInfoBuilder()
            : this(new FieldDescription[0])
        {
        }

        private RecordInfoBuilder(FieldDescription[] fields)
        {
            this._fieldsDescriptions = fields;
        }

        /// <summary>
        /// Adds an RecordInfo set of fields to the builder
        /// </summary>
        /// <param name="input">RecordInfo to copy</param>
        /// <param name="removeNameMatches">Remove duplicate names</param>
        /// <returns>A new configured field builder</returns>
        public RecordInfoBuilder AddFieldsFromRecordInfo(RecordInfo input, bool removeNameMatches = false)
        {
            var fields = Enumerable.Range(0, (int)input.NumFields())
                .Select(i => input[i])
                .Select(f => new FieldDescription(f))
                .ToArray();
            return this.AddFields(fields, removeNameMatches);
        }

        /// <summary>
        /// Adds a set of fields to the builder
        /// </summary>
        /// <param name="fields">Fields to add</param>
        /// <returns>A new configured field builder</returns>
        public RecordInfoBuilder AddFields(params FieldDescription[] fields) => this.AddFields(fields, false);

        /// <summary>
        /// Adds a set of fields to the builder
        /// </summary>
        /// <param name="fields">Fields to add</param>
        /// <param name="removeNameMatches">Remove duplicate names</param>
        /// <returns>A new configured field builder</returns>
        public RecordInfoBuilder AddFields(FieldDescription[] fields, bool removeNameMatches)
        {
            IEnumerable<FieldDescription> output = this._fieldsDescriptions;
            if (removeNameMatches)
            {
                var names = fields.ToDictionary(f => f.Name, f => 1);
                output = output.Where(f => !names.ContainsKey(f.Name));
            }

            return new RecordInfoBuilder(output.Concat(fields).ToArray());
        }

        /// <summary>
        /// Removes a set of fields to the builder
        /// </summary>
        /// <param name="fields">Field Names to remove</param>
        /// <returns>A new configured field builder</returns>
        public RecordInfoBuilder RemoveFields(IEnumerable<string> fields)
        {
            var names = fields.ToDictionary(f => f, f => 1);
            return new RecordInfoBuilder(this._fieldsDescriptions.Where(f => !names.ContainsKey(f.Name)).ToArray());
        }

        /// <summary>
        ///     Create a new RecordInfo with specified fields.
        /// </summary>
        /// <returns>A configured RecordInfo object.</returns>
        public RecordInfo Build()
        {
            var output = new RecordInfo();

            foreach (var descr in this._fieldsDescriptions)
            {
                output.AddField(descr.Name, descr.FieldType, (int)descr.ParsedSize, descr.Scale, descr.Source, descr.Description);
            }

            return output;
        }
    }
}