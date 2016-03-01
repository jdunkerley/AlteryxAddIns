namespace JDunkerley.Alteryx.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using AlteryxGuiToolkit.Plugins;

    using AlteryxRecordInfoNet;

    using JDunkerley.Alteryx.Attributes;

    public static class Utilities
    {
        /// <summary>
        /// Gets the incoming connections.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Dictionary<string, PropertyInfo> GetConnections<T>(this Type type)
        {
            var properties =
                type
                    .GetProperties()
                    .Where(p => typeof(T).IsAssignableFrom(p.PropertyType));

            var connections = new Dictionary<string, PropertyInfo>();
            foreach (var property in properties)
            {
                connections[property.Name] = property;
            }

            return connections;
        }

        public static IEnumerable<Connection> ToConnections(
            this IEnumerable<KeyValuePair<string, PropertyInfo>> connections)
        {
            return connections
                .OrderBy(kvp => kvp.Value.GetAttrib<OrderingAttribute>()?.Order ?? int.MaxValue)
                .ThenBy(kvp => kvp.Key)
                .Select(
                        kvp =>
                        new Connection(
                            kvp.Key,
                            kvp.Key,
                            false,
                            kvp.Value.GetAttrib<OptionalAttribute>() != null,
                            kvp.Value.GetAttrib<CharLabelAttribute>()?.Label))
                    .ToArray();
        }

        public static T GetAttrib<T>(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(typeof(T), true).Cast<T>().FirstOrDefault();
        }

        public static RecordCopier CreateCopier(RecordInfo info, RecordInfo newRecordInfo, params string[] fieldsToSkip)
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
            return copier;
        }

        public static RecordInfo CreateRecordInfo(
            params FieldDescription[] fields)
        {
            return CreateRecordInfo(null, fields);
   ;     }
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
