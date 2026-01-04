using System;
using System.IO;

namespace redwyre.DevTools.Editor.Terminal
{
    /// <summary>
    /// Console host interface for the terminal frontend.
    /// </summary>
    public interface IConsole
    {
        public string DisplayName { get; }

        public bool SupportsVT100 { get; }

        public bool HasTerminated { get; }

        public StreamReader ConsoleOutputStream { get; }

        public StreamWriter ConsoleInputStream { get; }
    }
}
