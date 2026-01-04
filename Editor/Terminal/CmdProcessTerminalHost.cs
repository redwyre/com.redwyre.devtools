namespace redwyre.DevTools.Editor.Terminal
{
    public sealed class CmdProcessTerminalHost : ProcessTerminalHostBase
    {
        public override string DisplayName => "cmd";

        public CmdProcessTerminalHost()
            : base("cmd.exe")
        {
        }
    }
}
