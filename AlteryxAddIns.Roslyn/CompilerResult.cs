namespace JDunkerley.AlteryxAddIns.Roslyn
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using Microsoft.CodeAnalysis;

    public class CompilerResult
    {
        public bool Success { get; set; }

        public Type ReturnType { get; set; }

        public MethodInfo Execute { get; set; }

        public List<Diagnostic> Messages { get; set; }
    }
}