using System;
using System.Collections.Generic;

namespace OmniBus.HTMLHelper
{
    internal interface IEngineScanner
    {
        IReadOnlyList<Type> EngineTypes { get; }

        IEnumerable<Connection> InputConnections(Type engineType);
        IEnumerable<Connection> OutputConnections(Type engineType);
    }
}