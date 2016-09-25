namespace JDunkerley.AlteryxAddIns
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using AlteryxRecordInfoNet;

    using JDunkerley.AlteryxAddIns.Framework;
    using JDunkerley.AlteryxAddIns.Framework.Attributes;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    public class RoslynInput :
        BaseTool<RoslynInput.Config, RoslynInput.Engine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        private class CompilerResult
        {
            public bool Success { get; set; }

            public Type ReturnType { get; set; }

            public MethodInfo Execute { get; set; }

            public List<string> Messages { get; set; }
        }

        private static class Compiler
        {
            private static readonly PortableExecutableReference[] references;

            private static readonly ConcurrentDictionary<string, CompilerResult> results;

            static Compiler()
            {
                references = new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) // System
                                 };
                results = new ConcurrentDictionary<string, CompilerResult>();
            }

            private static SyntaxTree BuildSyntaxTree(string lambda)
            {
                var code = $@"
using System;

namespace Temporary
{{
    public class ExpressionClass
    {{
        public static object Execute() => Lambda({lambda})();

        public static object Lambda()
            => Lambda({lambda});


        private static Func<T> Lambda<T>(Func<T> lambda)
        {{
            return lambda;
        }}
    }}
}}
";
                return CSharpSyntaxTree.ParseText(code);
            }

            public static CompilerResult Compile(string lambda) =>
                results.GetOrAdd(lambda, DoCompilation);

            private static CompilerResult DoCompilation(string lambda)
            {

                var csharpSyntaxTree = BuildSyntaxTree(lambda);

                var compilation = CSharpCompilation.Create(
                    Path.GetRandomFileName(),
                    new[] { csharpSyntaxTree },
                    references,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                using (var ms = new MemoryStream())
                {
                    var emitResult = compilation.Emit(ms);
                    var output = new CompilerResult
                                     {
                                         Success = emitResult.Success,
                                         Messages =
                                             emitResult.Diagnostics.Select(d => $"{d.DefaultSeverity}: {d.GetMessage()}")
                                                 .ToList()
                                     };


                    if (emitResult.Success)
                    {
                        var type = Assembly.Load(ms.GetBuffer())
                            .GetTypes()
                            .FirstOrDefault(t => t.Name == "ExpressionClass");

                        output.ReturnType = type
                            ?.GetMethod("Lambda", BindingFlags.Static | BindingFlags.Public)
                            ?.Invoke(null, new object[0])
                            ?.GetType().GetGenericArguments()[0];

                        output.Execute = type
                            ?.GetMethod("Execute", BindingFlags.Static | BindingFlags.Public);
                    }

                    return output;
                }
            }

        }

        public class Config
        {
            /// <summary>
            /// Gets or sets the type of the output.
            /// </summary>
            [Category("Output")]
            [Description("Lambda Code for C#")]
            [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
            public string LambdaCode { get; set; } = "() => new { A = \"Hello World\" }";
        }

        public class Engine : BaseEngine<Config>
        {
            /// <summary>
            /// Constructor for Alteryx Engine
            /// </summary>
            public Engine()
                : base(null)
            {
            }


            /// <summary>
            /// Gets or sets the output.
            /// </summary>
            [CharLabel('O')]
            public OutputHelper Output { get; set; }

            /// <summary>
            /// Called only if you have no Input Connections
            /// </summary>
            /// <param name="nRecordLimit"></param>
            /// <returns></returns>
            public override bool PI_PushAllRecords(long nRecordLimit)
            {
                if (this.Output == null)
                {
                    this.Engine.OutputMessage(
                        this.NToolId,
                        AlteryxRecordInfoNet.MessageStatus.STATUS_Error,
                        "Output is not set.");
                    return false;
                }

                var code = this.ConfigObject.LambdaCode;
                var result = Compiler.Compile(code);

                foreach (var resultMessage in result.Messages)
                {
                    this.Engine.OutputMessage(this.NToolId, MessageStatus.STATUS_Info, resultMessage);
                }

                if (!result.Success)
                {
                    this.Engine.OutputMessage(
                        this.NToolId,
                        AlteryxRecordInfoNet.MessageStatus.STATUS_Error,
                        "Compilation Failed.");
                    return false;
                }

                var sampleType = result.ReturnType;

                var descriptions = FieldDescriptionsFromType(sampleType);
                var recordInfo = FieldDescription.CreateRecordInfo(descriptions.ToArray());
                this.Output.Init(recordInfo);

                if (nRecordLimit == 0)
                {
                    this.Output.Close(true);
                    return true;
                }

                var data = result.Execute.Invoke(null, new object[0]);
                var asEnum = (data as IEnumerable)?.Cast<object>().ToArray() ?? new[] { data };

                for (int index = 0; index < asEnum.Length; index++)
                {
                    var sample = asEnum[index];
                    var recordOut = this.Output.CreateRecord();

                    // To Do Set Values
                    foreach (var fieldDescription in descriptions)
                    {
                        var prop = sample.GetType().GetProperty(fieldDescription.Name);
                        var value = prop.GetValue(sample);
                        if (value == null)
                        {
                            this.Output[fieldDescription.Name].SetNull(recordOut);
                            continue;
                        }

                        switch (prop.PropertyType.Name)
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

                    this.Output.Push(recordOut);
                    this.Output.UpdateProgress((double)index / asEnum.Length);
                }
                this.ExecutionComplete();
                this.Output.Close(true);
                return true;
            }

            private static List<FieldDescription> FieldDescriptionsFromType(Type sampleType, string prefix = "")
            {
                // Handle Enumerable Types
                var iEnumerableType =
                    sampleType == typeof(string) ? null :
                    sampleType.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        .Select(i => i.GenericTypeArguments[0])
                        .FirstOrDefault();
                if (iEnumerableType != null)
                {
                    return FieldDescriptionsFromType(iEnumerableType);
                }

                var descriptions = new List<FieldDescription>();

                // Handle A Primitive Type
                var primitive = FieldDescription.FromNameAndType("Value", sampleType);
                if (primitive != null)
                {
                    descriptions.Add(primitive);
                    return descriptions;
                }

                // Add All Properties
                foreach (var propertyInfo in sampleType.GetProperties())
                {
                    if (!propertyInfo.CanRead)
                    {
                        continue;
                    }

                    var description = FieldDescription.FromNameAndType(prefix + propertyInfo.Name, propertyInfo.PropertyType);
                    if (description == null)
                    {
                        // ToDo: Recurse ... descriptions.AddRange(FieldDescriptionsFromType(propertyInfo.PropertyType, (prefix == "" ? "" : $"{prefix}.") + propertyInfo.Name + "."));
                        continue;
                    }

                    description.Source = nameof(RoslynInput);
                    descriptions.Add(description);
                }
                return descriptions;
            }
        }
    }
}
