using System;

/// <summary>
/// Minimal host interface for a terminal backend.
/// </summary>
public interface IHost
{
    event Action<string> OutputChanged;
    void Write(string text);
    void WriteLine(string line);
    void Clear();
    void ExecuteCommand(string command);
}