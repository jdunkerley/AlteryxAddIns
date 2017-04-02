using System;
using System.IO;

namespace OmniBus.HTMLHelper
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            // Wire Up Assembly Resolve. Must be done before everything else as need to get dlls from Alteryx bin and embedded
            new AssemblyResolver().Init();

            var commandLineArgs = new CommandLineArgs();
            if (commandLineArgs.Parse(args))
            {
                return -1;
            }
            if (!File.Exists(commandLineArgs.DLLFile))
            {
                Console.WriteLine($"Could not find file {commandLineArgs.DLLFile}");
                return -1;
            }

            // Scan for Types
            var assembly = new AssemblyResolver(commandLineArgs.DLLFile).ReflectionLoad(args[0]);
            var scanner = new EngineScanner(assembly);
            foreach (var engineType in scanner.EngineTypes)
            {
                var xmlConfig = new XMLConfig(engineType, scanner);
                Console.WriteLine(xmlConfig.ConfigDocument.OuterXml);
            }

            return 0;
        }
    }
}