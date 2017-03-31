using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using OmniBus.Framework.Attributes;
using OmniBus.Framework.Interfaces;

namespace OmniBus.HTMLHelper
{
    internal class EngineScanner : IEngineScanner
    {
        private readonly Func<Type, bool> _isInput;
        private readonly Func<Type, bool> _isOutput;

        public EngineScanner(Assembly assembly)
        {
            this.EngineTypes = assembly.GetTypes()
                .Where(HasInterface(typeof(AlteryxRecordInfoNet.INetPlugin).FullName)) // Must be an INetPlugin to be an Alteryx .Net Engine
                .Where(t => t.GetConstructor(new Type[0]) != null) // Alteryx Needs A Default Constructor
                .ToList()
                .AsReadOnly();

            this._isInput = HasInterface(typeof(AlteryxRecordInfoNet.IIncomingConnectionInterface).FullName);
            this._isOutput = HasInterface(typeof(IOutputHelper).FullName);
        }

        public IReadOnlyList<Type> EngineTypes { get; }

        public IEnumerable<Connection> InputConnections(Type engineType)
        {
            var props = engineType.GetProperties();

            // Input Properties
            return props.Where(p => this._isInput(p.PropertyType))
                .OrderBy(GetOrdering)
                .ThenBy(p => p.Name)
                .Select(p => new Connection(p.Name, label: GetCharacterLabel(p)));
        }

        public IEnumerable<Connection> OutputConnections(Type engineType)
        {
            var props = engineType.GetProperties();

            // Input Properties
            return props.Where(p => this._isOutput(p.PropertyType))
                .OrderBy(GetOrdering)
                .ThenBy(p => p.Name)
                .Select(p => new Connection(p.Name, label: GetCharacterLabel(p)));
        }

        private static int GetOrdering(PropertyInfo property)
        {
            return property.GetCustomAttributesData()
                       .Where(a => a.AttributeType.FullName == typeof(OrderingAttribute).FullName)
                       .Select(a => a.ConstructorArguments[0].Value as int?)
                       .FirstOrDefault() ?? int.MaxValue;
        }

        private static char GetCharacterLabel(PropertyInfo property)
        {
            return property.GetCustomAttributesData()
                       .Where(a => a.AttributeType.FullName == typeof(CharLabelAttribute).FullName)
                       .Select(a => a.ConstructorArguments[0].Value as char?)
                       .FirstOrDefault() ?? Char.MinValue;
        }

        private static Func<Type, bool> HasInterface(string interfaceName)
        {
            return t => t.GetInterfaces().Any(i => i.FullName == interfaceName);
        }
    }
}