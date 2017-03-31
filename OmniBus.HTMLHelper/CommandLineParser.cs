using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fclp;

namespace OmniBus.HTMLHelper
{
    public class CommandLineParser
    {
        private readonly FluentCommandLineParser parser;

        public CommandLineParser()
        {

            this.parser = new FluentCommandLineParser();

            this.parser.IsCaseSensitive = false;

            this.parser.Setup<string>('d', "dll")
                .Callback(s => this.DLLFile = s)
                .Required()
                .WithDescription("DLL File To Scan For Engines");

            this.parser.SetupHelp("?", "help").Callback(text => Console.WriteLine(text));

        }

        public string DLLFile { get; private set; }
    }
}
