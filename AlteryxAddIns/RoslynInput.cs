namespace JDunkerley.AlteryxAddIns
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;

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

            public MethodInfo Lambda { get; set; }
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
        public static object Evaluate()
        {{
            Func<object> lambda = {lambda};
            return lambda();
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
                    var output = new CompilerResult { Success = emitResult.Success };

                    if (emitResult.Success)
                    {
                        var assembly = Assembly.Load(ms.GetBuffer());
                        output.Lambda = assembly
                            .GetTypes()
                            .Where(t => t.Name == "ExpressionClass")
                            .FirstOrDefault()
                            ?.GetMethod("Evaluate", BindingFlags.Static | BindingFlags.Public);
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
                var sample = result.Lambda.Invoke(null, new object[0]);
                var sampleType = sample.GetType();

                var descriptions = FieldDescriptionsFromType(sampleType);
                var recordInfo = FieldDescription.CreateRecordInfo(descriptions.ToArray());
                this.Output.Init(recordInfo);

                if (nRecordLimit == 0)
                {
                    this.Output.Close(true);
                    return true;
                }

                var recordOut = this.Output.CreateRecord();

                // To Do Set Values
                foreach (var fieldDescription in descriptions)
                {
                    var prop = sampleType.GetProperty(fieldDescription.Name);
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
                            this.Output[fieldDescription.Name].SetFromString(recordOut, ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss"));
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
                            this.Output[fieldDescription.Name].SetFromInt32(recordOut, (long)value);
                            break;
                        case nameof(Double):
                            this.Output[fieldDescription.Name].SetFromInt32(recordOut, (double)value);
                            break;
                        case nameof(Single):
                            this.Output[fieldDescription.Name].SetFromInt32(recordOut, (float)value);
                            break;
                        default:
                            continue;

                    }
                }

                this.Output.Push(recordOut);
                this.Output.UpdateProgress(1.0);
                this.Output.Close(true);
                return true;
            }

            private static List<FieldDescription> FieldDescriptionsFromType(Type sampleType)
            {
                var descriptions = new List<FieldDescription>();
                foreach (var propertyInfo in sampleType.GetProperties())
                {
                    if (!propertyInfo.CanRead)
                    {
                        continue;
                    }

                    FieldDescription description;
                    switch (propertyInfo.PropertyType.Name)
                    {
                        case nameof(String):
                            description = OutputType.VWString.OutputDescription(propertyInfo.Name, 32000);
                            break;
                        case nameof(DateTime):
                            description = OutputType.DateTime.OutputDescription(propertyInfo.Name, 19);
                            break;
                        case nameof(Boolean):
                            description = OutputType.Bool.OutputDescription(propertyInfo.Name, 0);
                            break;
                        case nameof(Byte):
                            description = OutputType.Byte.OutputDescription(propertyInfo.Name, 0);
                            break;
                        case nameof(Int16):
                            description = OutputType.Int16.OutputDescription(propertyInfo.Name, 0);
                            break;
                        case nameof(Int32):
                            description = OutputType.Int32.OutputDescription(propertyInfo.Name, 0);
                            break;
                        case nameof(Int64):
                            description = OutputType.Int64.OutputDescription(propertyInfo.Name, 0);
                            break;
                        case nameof(Double):
                            description = OutputType.Double.OutputDescription(propertyInfo.Name, 0);
                            break;
                        case nameof(Single):
                            description = OutputType.Float.OutputDescription(propertyInfo.Name, 0);
                            break;
                        default:
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
