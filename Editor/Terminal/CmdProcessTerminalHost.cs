using System.Diagnostics;

public sealed class CmdProcessTerminalHost : ProcessTerminalHostBase
{
    public override string ShellDisplayName => "cmd.exe";

    protected override ProcessStartInfo CreateStartInfo(string command)
    {
        return Configure(new ProcessStartInfo("cmd.exe", "/c " + command));
    }
}
