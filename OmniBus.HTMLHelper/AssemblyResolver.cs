using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Win32;

namespace OmniBus.HTMLHelper
{
    internal class AssemblyResolver
    {
        private readonly string[] _paths;

        public AssemblyResolver(string assemblyFileName = null)
        {
            this._paths = new[]
                              {
                                  Path.GetDirectoryName(assemblyFileName),
                                  Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                  Environment.CurrentDirectory,
                                  AlteryxDirectory(),
                                  Path.GetDirectoryName(typeof(string).Assembly.Location)
                              };
        }

        public void SetUpAssemblyResolver()
        {
            var execAssembly = Assembly.GetExecutingAssembly();
            var embedded = execAssembly.GetManifestResourceNames();

            byte[] GetAssembly(string name)
            {
                var stream = execAssembly.GetManifestResourceStream(name);
                if (stream == null)
                {
                    return null;
                }

                using (stream)
                {
                    var data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);
                    return data;
                }
            }

            AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
                {
                    var assemblyName = new AssemblyName(eventArgs.Name);
                    var resource = embedded.Where(e => e.EndsWith($"{assemblyName.Name}.dll")).Select(GetAssembly).FirstOrDefault();
                    if (resource != null)
                    {
                        return Assembly.Load(resource);
                    }

                    var path = this.GetPath(eventArgs.Name, this._paths);
                    return path != null ? Assembly.LoadFrom(path) : null;
                };
        }

        public Assembly ReflectionLoad(string assemblyFileName)
        {
            Assembly AssemblyResolve(object sender, ResolveEventArgs eventArgs)
            {
                var path = this.GetPath(eventArgs.Name, this._paths);
                return path != null ? Assembly.ReflectionOnlyLoadFrom(path) : null;
            }

            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += AssemblyResolve;
            return Assembly.ReflectionOnlyLoadFrom(assemblyFileName);
        }

        private string GetPath(string fullName, string[] paths)
        {
            var assemblyName = new AssemblyName(fullName);
            var path = paths.Where(p => p != null)
                .Select(p => Path.Combine(p, $"{assemblyName.Name}.dll"))
                .FirstOrDefault(File.Exists);
            return path;
        }

        private static string AlteryxDirectory() => (Registry.GetValue(
                                                         "HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\SRC\\Alteryx",
                                                         "InstallDir64",
                                                         null) ?? Registry.GetValue(
                                                         "HKEY_CURRENT_USER\\SOFTWARE\\SRC\\Alteryx",
                                                         "InstallDir64",
                                                         null))?.ToString();

    }
}