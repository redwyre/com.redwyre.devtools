namespace redwyre.DevTools.Editor.Terminal
{
    public sealed class BashProcessTerminalHost : ProcessTerminalHostBase
    {
        public override string DisplayName => "bash";

        public BashProcessTerminalHost()
            : base("/bin/bash")
        {
        }
    }
}
