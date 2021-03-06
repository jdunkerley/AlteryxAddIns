using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.Factories;
using OmniBus.Framework.Interfaces;
using OmniBus.Framework.Serialisation;

namespace JDunkerley.AlteryxAddIns.Roslyn
{
    public class RoslynInputEngine : BaseEngine<RoslynInputConfig>
    {
        private CompilerResult _result;

        private List<FieldDescription> _descriptions;

        /// <summary>
        /// Constructor for Alteryx Engine
        /// </summary>
        public RoslynInputEngine()
            : this(new OutputHelperFactory())
        {
        }

        /// <summary>
        /// Create An Engine for unit testing.
        /// </summary>
        /// <param name="outputHelperFactory">Factory to create output helpers</param>
        internal RoslynInputEngine(IOutputHelperFactory outputHelperFactory)
            : base(null, outputHelperFactory)
        {
        }

        /// <summary>
        /// Gets or sets the output.
        /// </summary>
        [CharLabel('O')]
        public IOutputHelper Output { get; set; }

        /// <summary>
        /// Given a block of C# wrap with needed code
        /// </summary>
        /// <param name="lambda"></param>
        /// <returns></returns>
        public static string GetCodeFromLambda(string lambda) => string.IsNullOrWhiteSpace(lambda) ? null : $@"
using System;
using System.Collections.Generic;
using System.Linq;

namespace Temporary
{{
    public class ExpressionClass
    {{
        public static Delegate CreateDelegate()
            => MakeFunc({lambda});


        private static Func<T> MakeFunc<T>(Func<T> code)
        {{
            return code;
        }}
    }}
}}";


        /// <summary>
        /// Called only if you have no Input Connections
        /// </summary>
        /// <param name="nRecordLimit"></param>
        /// <returns></returns>
        public override bool PI_PushAllRecords(long nRecordLimit)
        {
            if (!this._result.Success)
            {
                return false;
            }

            if (this.Output == null)
            {
                this.Engine.OutputMessage(
                    this.NToolId,
                    MessageStatus.STATUS_Error,
                    "Output is not set.");
                return false;
            }

            var recordInfo = FieldDescription.CreateRecordInfo(this._descriptions.ToArray());
            this.Output.Init(recordInfo);

            if (nRecordLimit == 0)
            {
                this.Output.Close(true);
                return true;
            }

            var data = this._result.Execute.DynamicInvoke();
            var recordOut = this.Output.Record;

            var asString = data as string;
            var asEnum = asString != null ? new[] { asString } : (data as IEnumerable) ?? new[] { data };
            foreach (object sample in asEnum)
            {
                this.PushRecord(recordOut, this._descriptions, sample);
            }

            this.ExecutionComplete();
            this.Output.Close(true);
            return true;
        }

        /// <summary>Create a Serialiser</summary>
        /// <returns><see cref="T:OmniBus.Framework.Serialisation.ISerialiser`1" /> to de-serialise object</returns>
        protected override ISerialiser<RoslynInputConfig> Serialiser() => new Serialiser<RoslynInputConfig>();

        protected override void OnInitCalled()
        {
            var code = GetCodeFromLambda(this.ConfigObject.LambdaCode);
            this._result = Compiler.Compile(code);

            foreach (var resultMessage in this._result.Messages)
            {
                this.Engine.OutputMessage(this.NToolId, MessageStatus.STATUS_Info, resultMessage.GetMessage());
            }

            if (!this._result.Success)
            {
                this.Engine.OutputMessage(
                    this.NToolId,
                    MessageStatus.STATUS_Error,
                    "Compilation Failed.");
                return;
            }

            var sampleType = this._result.ReturnType;

            this._descriptions = FieldDescriptionsFromType(sampleType);
        }

        private void PushRecord(Record recordOut, List<FieldDescription> descriptions, object sample)
        {
            recordOut.Reset();

            // To Do Set Values
            foreach (var fieldDescription in descriptions)
            {
                object value;
                string valueTypeName;

                if (fieldDescription.Description?.StartsWith("BaseValue: ") ?? false)
                {
                    value = sample;
                    valueTypeName = fieldDescription.Description.Substring(11);
                }
                else
                {
                    var prop = sample.GetType().GetProperty(fieldDescription.Name);
                    value = prop.GetValue(sample);
                    valueTypeName = prop.PropertyType.Name;
                }

                if (value == null)
                {
                    this.Output[fieldDescription.Name].SetNull(recordOut);
                    continue;
                }

                switch (valueTypeName)
                {
                    case nameof(String):
                        this.Output[fieldDescription.Name].SetFromString(recordOut, (string)value);
                        break;
                    case nameof(DateTime):
                        this.Output[fieldDescription.Name].SetFromString(
                            recordOut,
                            ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss"));
                        break;
                    case nameof(Boolean):
                        this.Output[fieldDescription.Name].SetFromBool(recordOut, (bool)value);
                        break;
                    case nameof(Byte):
                        this.Output[fieldDescription.Name].SetFromInt32(recordOut, (byte)value);
                        break;
                    case nameof(Int16):
                        this.Output[fieldDescription.Name].SetFromInt32(recordOut, (short)value);
                        break;
                    case nameof(Int32):
                        this.Output[fieldDescription.Name].SetFromInt32(recordOut, (int)value);
                        break;
                    case nameof(Int64):
                        this.Output[fieldDescription.Name].SetFromInt64(recordOut, (long)value);
                        break;
                    case nameof(Double):
                        this.Output[fieldDescription.Name].SetFromDouble(recordOut, (double)value);
                        break;
                    case nameof(Single):
                        this.Output[fieldDescription.Name].SetFromDouble(recordOut, (float)value);
                        break;
                    default:
                        continue;
                }
            }

            this.Output.Push(recordOut, updateCountMod: 100);
        }

        private static List<FieldDescription> FieldDescriptionsFromType(Type sampleType, string prefix = "")
        {
            var descriptions = new List<FieldDescription>();

            // Handle A Primitive Type
            var primitiveType = FieldDescription.GetAlteryxFieldType(sampleType);
            if (primitiveType != FieldType.E_FT_Unknown)
            {
                descriptions.Add(
                    new FieldDescription(
                        "Value",
                        primitiveType,
                        source: $"RoslynInput: {sampleType.Name}")
                        {
                            Description = $"BaseValue: {sampleType.Name}"
                        });
                return descriptions;
            }

            // Handle Enumerable Types
            var iEnumerableType = sampleType.IsInterface
                                  && sampleType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                                      ? sampleType.GenericTypeArguments[0]
                                      : sampleType.GetInterfaces()
                                          .Where(
                                              i =>
                                                  i.IsGenericType
                                                  && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                                          .Select(i => i.GenericTypeArguments[0])
                                          .FirstOrDefault();
            if (iEnumerableType != null)
            {
                return FieldDescriptionsFromType(iEnumerableType);
            }

            // Add All Properties
            foreach (var propertyInfo in sampleType.GetProperties())
            {
                if (!propertyInfo.CanRead)
                {
                    continue;
                }

                var descriptionType = FieldDescription.GetAlteryxFieldType(propertyInfo.PropertyType);
                if (descriptionType == FieldType.E_FT_Unknown)
                {
                    // ToDo: Recurse ... descriptions.AddRange(FieldDescriptionsFromType(propertyInfo.PropertyType, (prefix == "" ? "" : $"{prefix}.") + propertyInfo.Name + "."));
                    continue;
                }

                descriptions.Add(
                    new FieldDescription(
                        prefix + propertyInfo.Name,
                        descriptionType,
                        source: "RoslynInput"));
            }
            return descriptions;
        }
    }
}