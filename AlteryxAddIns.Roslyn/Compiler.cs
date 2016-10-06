namespace JDunkerley.AlteryxAddIns.Roslyn
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    public static class Compiler
    {
        private static readonly PortableExecutableReference[] References;

        private static readonly ConcurrentDictionary<string, CompilerResult> Results;

        static Compiler()
        {
            References = new[]
                             {
                                 typeof(object), // System
                                 typeof(Enumerable), // System.Core,
                                 typeof(System.Xml.XmlDocument), // System.Xml,
                                 typeof(System.Data.DataTable) // System.Data
                             }.Select(t => MetadataReference.CreateFromFile(t.Assembly.Location)).ToArray();
            Results = new ConcurrentDictionary<string, CompilerResult>();
        }

        private static SyntaxTree BuildSyntaxTree(string code)
        {
            return CSharpSyntaxTree.ParseText(code);
        }

        public static CompilerResult Compile(string code) =>
            !string.IsNullOrWhiteSpace(code) ? Results.GetOrAdd(code, DoCompilation) : new CompilerResult { Success = false, Messages = new List<Diagnostic>() };

        private static CompilerResult DoCompilation(string code)
        {
            var csharpSyntaxTree = BuildSyntaxTree(code);

            var compilation = CSharpCompilation.Create(
                Path.GetRandomFileName(),
                new[] { csharpSyntaxTree },
                References,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                var emitResult = compilation.Emit(ms);
                var output = new CompilerResult
                                 {
                                     Success = emitResult.Success,
                                     Messages = emitResult.Diagnostics.ToList()
                                 };


                if (emitResult.Success)
                {
                    var type = Assembly.Load(ms.GetBuffer())
                        .GetTypes()
                        .FirstOrDefault(t => t.Name == "ExpressionClass");

                    var delegateObject = type
                        ?.GetMethod("CreateDelegate", BindingFlags.Static | BindingFlags.Public)
                        ?.Invoke(null, new object[0]) as System.Delegate;

                    output.ReturnType = delegateObject?.GetType().GetGenericArguments()[0];
                    output.Execute = delegateObject;
                }

                return output;
            }
        }

    }
}