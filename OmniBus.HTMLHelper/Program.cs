using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace OmniBus.HTMLHelper
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            // Wire Up Assembly Resolve. Must be done before everything else as need to get dlls from Alteryx bin and embedded
            new AssemblyResolver().Init();

            if (args.Length == 0)
            {
                Console.WriteLine("Syntax: <PlugInDll> [OutputFolder]");
                return -1;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine($"Could not find file {args[0]}");
                return -1;
            }

            // Scan for Types
            var assembly = new AssemblyResolver(args[0]).ReflectionLoad(args[0]);
            var scanner = new EngineScanner(assembly);
            foreach (var engineType in scanner.EngineTypes)
            {
                var xmlDoc = CreateConfigXml(args, engineType, scanner);
                Console.WriteLine(xmlDoc.OuterXml);
            }

            return 0;
        }
    }
}