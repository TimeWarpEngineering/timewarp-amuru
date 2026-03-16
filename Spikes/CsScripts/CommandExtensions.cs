#region Purpose
// TODO: Add purpose description
#endregion

using System;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;

public static class CommandExtensions
{
    public static CommandResult Run(string command, params string[] args)
    {
        var cliCommand = Cli.Wrap(command)
            .WithArguments(args)
            .WithValidation(CommandResultValidation.None);
            
        return new CommandResult(cliCommand);
    }
}

public class CommandResult
{
    private readonly Command _command;
    
    internal CommandResult(Command command)
    {
        _command = command;
    }
    
    public async Task<string> GetStringAsync()
    {
        var result = await _command.ExecuteBufferedAsync();
        return result.StandardOutput;
    }
    
    public async Task<string[]> GetLinesAsync()
    {
        var result = await _command.ExecuteBufferedAsync();
        return result.StandardOutput.Split(
            new[] { '\n', '\r' }, 
            StringSplitOptions.RemoveEmptyEntries
        );
    }
    
    public async Task ExecuteAsync()
    {
        await _command.ExecuteAsync();
    }
}