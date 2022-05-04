using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Tasks;

namespace Guilded.Commands;

/// <summary>
/// Represents the base for all <see cref="CommandAttribute">command types</see>.
/// </summary>
public class CommandBase
{
    private readonly Subject<FailedCommandEvent> onFailedCommand = new();
    /// <summary>
    /// Gets the list of commands or sub-commands of this command.
    /// </summary>
    /// <value>Commands</value>
    public IEnumerable<ICommandInfo<MemberInfo>> Commands { get; protected set; }
    /// <summary>
    /// Gets the event for failed command invokation.
    /// </summary>
    /// <returns>Observable</returns>
    public IObservable<FailedCommandEvent> FailedCommand => onFailedCommand.AsObservable();
    /// <summary>
    /// Initializes a new instance of <see cref="CommandBase" />.
    /// </summary>
    /// <param name="commands">The sub-commands of this command</param>
    public CommandBase(IEnumerable<ICommandInfo<MemberInfo>> commands) =>
        Commands = commands;
    /// <summary>
    /// Invokes any of the command's <see cref="Commands">sub-commands</see>.
    /// </summary>
    /// <param name="usedBaseName">The specified name of this command</param>
    /// <param name="context">The information about the original command</param>
    /// <param name="arguments">The arguments given to this command</param>
    public async Task InvokeAsync(string usedBaseName, RootCommandContext context, IEnumerable<string> arguments)
    {
        if (!arguments.Any())
        {
            // Command index
            CommandEvent thisParentEvent = new(context.MessageEvent, context.Prefix, context.RootCommandName, context.RootArguments, usedBaseName, arguments);
            onFailedCommand.OnNext(new FailedCommandEvent(thisParentEvent, FallbackType.Unspecified));
            return;
        }

        await InvokeAnyCommandAsync(context, commandName: arguments.First(), arguments: arguments.Skip(1)).ConfigureAwait(false);
    }
    /// <summary>
    /// Filters out commands that do not have <paramref name="name">the specified name</paramref>.
    /// </summary>
    /// <param name="name">The name of the commands to get</param>
    /// <returns>Filtered commands</returns>
    public IEnumerable<ICommandInfo<MemberInfo>> FilterCommandsByName(string name) =>
        Commands.Where(command => command.HasName(name));
    /// <summary>
    /// Filters <see cref="Commands">commands</see> and invokes any commands that were found. If none are found, <see cref="FailedCommand">failed command event</see> is invoked.
    /// </summary>
    /// <param name="context">The information about the original command</param>
    /// <param name="commandName">The name of the current command used</param>
    /// <param name="arguments">The arguments of the current command used</param>
    public async Task InvokeAnyCommandAsync(RootCommandContext context, string commandName, IEnumerable<string> arguments)
    {
        // Filter by parameters and names
        var filteredSubCommands =
            FilterCommandsByName(commandName)
                .Select(command =>
                    command is CommandContainerInfo commandContainer
                        ? (command, arguments, isContainer: true)
                        : (command, arguments: ((CommandInfo)command).GenerateMethodParameters(arguments), isContainer: true)
                )
                .Where(tuple => tuple.isContainer || tuple.arguments is not null);

        var firstTuple = filteredSubCommands.FirstOrDefault();
        var firstCommand = firstTuple.command;

        if (firstCommand is CommandContainerInfo commandContainer)
        {
            await commandContainer.Instance.InvokeAsync(commandName, context, arguments).ConfigureAwait(false);
            return;
        }

        // Context
        CommandEvent commandEvent = new(context.MessageEvent, context.Prefix, context.RootCommandName, context.RootArguments, commandName, arguments);

        if (firstCommand is CommandInfo command)
            await command.InvokeAsync(this, commandEvent, firstTuple.arguments!).ConfigureAwait(false);
        else onFailedCommand.OnNext(new FailedCommandEvent(commandEvent, FallbackType.NoCommandFound));
    }
}