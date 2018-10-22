using System.Linq;
using AlteryxRecordInfoNet;
using OmniBus.Framework.Interfaces;

namespace OmniBus.Framework.Builders
{
    /// <summary>
    ///     Factory For Creating <see cref="RecordCopier" /> objects, which are wrapped <see cref="IRecordCopier" />.
    /// </summary>
    public class RecordCopierBuilder
    {
        private readonly RecordInfo _inputRecordInfo;
        private readonly RecordInfo _outputRecordInfo;
        private string[] _fieldsToSkip = new string[0];

        /// <summary>
        ///     Initializes a new instance of the <see cref="RecordCopierBuilder"/> class.
        /// </summary>
        /// <param name="inputRecordInfo">The source <see cref="AlteryxRecordInfoNet.RecordInfo" /> object.</param>
        /// <param name="outputRecordInfo">The target <see cref="AlteryxRecordInfoNet.RecordInfo" /> objects.</param>
        public RecordCopierBuilder(RecordInfo inputRecordInfo, RecordInfo outputRecordInfo)
        {
            this._inputRecordInfo = inputRecordInfo;
            this._outputRecordInfo = outputRecordInfo;
        }

        /// <summary>
        /// Creates a new <see cref="RecordCopierBuilder"/> skipping specified fields.
        /// </summary>
        /// <param name="fieldsToSkip">Additional Fields To Skip</param>
        /// <returns>A new <see cref="RecordCopierBuilder"/> with additional fields skipped.</returns>
        public RecordCopierBuilder SkipFields(params string[] fieldsToSkip)
        {
            var newBuilder = new RecordCopierBuilder(this._inputRecordInfo, this._outputRecordInfo);
            newBuilder._fieldsToSkip = this._fieldsToSkip.Concat(this._fieldsToSkip).ToArray();
            return newBuilder;
        }

        /// <summary>
        /// Create an <see cref="IRecordCopier"/> to copy records.
        /// </summary>
        /// <returns>Configured Copier</returns>
        public IRecordCopier Build()
        {
            var copier = new RecordCopier(this._outputRecordInfo, this._inputRecordInfo, true);

            for (var fieldNum = 0; fieldNum < this._inputRecordInfo.NumFields(); fieldNum++)
            {
                var fieldName = this._inputRecordInfo[fieldNum].GetFieldName();
                if (this._fieldsToSkip.Contains(fieldName))
                {
                    continue;
                }

                var newFieldNum = this._outputRecordInfo.GetFieldNum(fieldName, false);
                if (newFieldNum == -1)
                {
                    continue;
                }

                copier.Add(newFieldNum, fieldNum);
            }

            copier.DoneAdding();
            return new RecordCopierWrapper(copier);
        }

        private class RecordCopierWrapper : IRecordCopier
        {
            private readonly RecordCopier _parent;

            public RecordCopierWrapper(RecordCopier parent)
            {
                this._parent = parent;
            }

            /// <summary>
            ///     Copy a RecordData onto a Record
            /// </summary>
            /// <param name="destination">Target Record Object</param>
            /// <param name="source">Source RecordData</param>
            public void Copy(Record destination, RecordData source) => this._parent.Copy(destination, source);
        }
    }
}