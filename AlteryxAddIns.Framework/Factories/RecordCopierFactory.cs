namespace JDunkerley.AlteryxAddIns.Framework.Factories
{
    using System.Linq;

    using AlteryxRecordInfoNet;

    using Interfaces;



    public class RecordCopierFactory : IRecordCopierFactory
    {
        /// <summary>
        /// Creates a record copier copier.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="newRecordInfo">The new record information.</param>
        /// <param name="fieldsToSkip">The fields to skip.</param>
        /// <returns></returns>
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
            /// <param name="source">Soruce RecordData</param>
            public void Copy(Record destination, RecordData source) => this._parent.Copy(destination, source);
        }
    }
}
