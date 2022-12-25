using Kook.API;
using Kook.API.Rest;

namespace Kook.Rest;

public static class RestGuildExperimentalExtensions
{
    /// <summary>
    ///     Deletes this guild.
    /// </summary>
    /// <param name="guild">The guild to delete.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>A task that represents the asynchronous deletion operation.</returns>
    /// <remarks>
    ///     <note type="warning">
    ///         This method is still in experimental state, which means that it is not for official API implementation
    ///         usage, may violate the developer rules or policies, not guaranteed to be stable, and may be changed or removed in the future.
    ///     </note>
    /// </remarks>
    public static Task DeleteAsync(this RestGuild guild, RequestOptions options = null)
        => ExperimentalGuildHelper.DeleteAsync(guild, guild.Kook, options);
    /// <summary>
    ///     Modifies this guild.
    /// </summary>
    /// <param name="guild">The guild to modify.</param>
    /// <param name="func">The delegate containing the properties to modify the guild with.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous modification operation.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="func"/> is <see langword="null"/>.</exception>
    public static async Task ModifyAsync(this RestGuild guild, Action<GuildProperties> func, RequestOptions options = null)
    {
        var model = await ExperimentalGuildHelper.ModifyAsync(guild, guild.Kook, func, options).ConfigureAwait(false);
        guild.Update(model);
    }
}