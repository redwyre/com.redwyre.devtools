using System;
using System.IO;

namespace redwyre.DevTools.Editor.Terminal.Win
{
    public sealed class ConPTY : IConsole
    {
        public string DisplayName => "ConPTY";

        public bool SupportsVT100 => true;

        public bool HasTerminated { get; }

        public StreamReader ConsoleOutputStream { get; }

        public StreamWriter ConsoleInputStream { get; }
    }
}