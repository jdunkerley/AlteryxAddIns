using System;

using Fclp;

namespace OmniBus.HTMLHelper
{
    public class CommandLineArgs
    {
        private readonly FluentCommandLineParser _parser;

        public CommandLineArgs()
        {

            this._parser = new FluentCommandLineParser { IsCaseSensitive = false };

            this._parser.Setup<string>('d', "dll")
                .Callback(s => this.DLLFile = s)
                .Required()
                .WithDescription("DLL File To Scan For Engines");

            this._parser.SetupHelp("?", "help").Callback(text => Console.WriteLine(text));
        }

        public bool Parse(string[] args)
        {
            var results = this._parser.Parse(args);
            if (results.HasErrors)
            {
                Console.WriteLine(results.ErrorText);
            }
            return results.HasErrors;
        }

        public string DLLFile { get; private set; }
    }
}
