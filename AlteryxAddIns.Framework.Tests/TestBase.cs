namespace JDunkerley.AlteryxAddIns.Framework.Tests
{
    using System;
    using System.Reflection;
    using System.Threading;

    public static class TestHelper
    {
        private static int _resolverSet;

        private static readonly Lazy<string> AlteryxPath = new Lazy<string>(GetAlteryxPath);

        private static string GetAlteryxPath()
        {
            var reg = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\SRC\Alteryx", "InstallDir64", null)
                ?? Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\SRC\Alteryx", "InstallDir64", null);
            return reg.ToString();
        }

        public static void InitResolver()
        {
            if (Interlocked.Exchange(ref _resolverSet, 1) == 0)
            {
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                    {
                        string fileName = $"{args.Name.Split(',')[0]}.dll";
                        fileName = System.IO.Path.Combine(AlteryxPath.Value, fileName);
                        return System.IO.File.Exists(fileName) ? Assembly.LoadFile(fileName) : null;
                    };
            }
        }

    }
}