namespace JDunkerley.AlteryxAddIns.Framework.Factories
{
    using System.Linq;

    using AlteryxRecordInfoNet;

    using Interfaces;

    /// <summary>
    /// Factory For Creating <see cref="IRecordCopier"/> objects, which are wrapped <see cref="RecordCopier"/>.
    /// </summary>
    public class RecordCopierFactory : IRecordCopierFactory
    {
        /// <summary>
        /// Creates a new instance of an <see cref="IRecordCopier"/>.
        /// </summary>
        /// <param name="info">The source <see cref="AlteryxRecordInfoNet.RecordInfo"/> object.</param>
        /// <param name="newRecordInfo">The target <see cref="AlteryxRecordInfoNet.RecordInfo"/> objects.</param>
        /// <param name="fieldsToSkip">A list of fields to skip.</param>
        /// <returns>A new instance of an <see cref="IRecordCopier"/> object.</returns>
        public IRecordCopier CreateCopier(RecordInfo info, RecordInfo newRecordInfo, params string[] fieldsToSkip)
        {
            var copier = new RecordCopier(newRecordInfo, info, true);

            for (int fieldNum = 0; fieldNum < info.NumFields(); fieldNum++)
            {
                string fieldName = info[fieldNum].GetFieldName();
                if (fieldsToSkip.Contains(fieldName))
                {
                    continue;
                }

                var newFieldNum = newRecordInfo.GetFieldNum(fieldName, false);
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
            /// Copy a RecordData onto a Record
            /// </summary>
            /// <param name="destination">Target Record Object</param>
            /// <param name="source">Source RecordData</param>
            public void Copy(Record destination, RecordData source) => this._parent.Copy(destination, source);
        }
    }
}
