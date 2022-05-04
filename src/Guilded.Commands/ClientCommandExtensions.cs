namespace Guilded.Commands;

/// <summary>
/// Extensions for Guilded clients related to commands.
/// </summary>
/// <remarks>
/// <para>Adds command-related extensions to clients.</para>
/// </remarks>
/// <seealso cref="CommandModule" />
public static class ClientCommandExtensions
{
    #region AddCommands
    /// <summary>
    /// Adds a command module to the client.
    /// </summary>
    /// <param name="client">The client to add command module to</param>
    /// <param name="commandModule">The command module to add to the client</param>
    /// <returns>Guilded client</returns>
    public static AbstractGuildedClient AddCommands(this AbstractGuildedClient client, CommandModule commandModule)
    {
        commandModule.AddTo(client);

        return client;
    }
    #endregion
}