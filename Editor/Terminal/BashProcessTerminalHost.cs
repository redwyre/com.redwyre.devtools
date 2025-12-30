using System.Diagnostics;

public sealed class BashProcessTerminalHost : ProcessTerminalHostBase
{
    public override string ShellDisplayName => "/bin/bash";

    protected override ProcessStartInfo CreateStartInfo(string command)
    {
        var escapedCommand = command.Replace("\"", "\\\"");
        return Configure(new ProcessStartInfo("/bin/bash", $"-lc \"{escapedCommand}\""));
    }
}