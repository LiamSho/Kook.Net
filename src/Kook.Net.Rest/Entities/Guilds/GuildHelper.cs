using Kook.API;
using Kook.API.Rest;
using System.Collections.Immutable;

namespace Kook.Rest;

internal static class GuildHelper
{
    #region General

    public static async Task LeaveAsync(IGuild guild, BaseKookClient client,
        RequestOptions options) =>
        await client.ApiClient.LeaveGuildAsync(guild.Id, options).ConfigureAwait(false);

    public static async Task<ImmutableDictionary<IUser, IReadOnlyCollection<BoostSubscriptionMetadata>>> GetBoostSubscriptionsAsync(
        IGuild guild, BaseKookClient client, RequestOptions options)
    {
        IEnumerable<BoostSubscription> subscriptions = await client.ApiClient
            .GetGuildBoostSubscriptionsAsync(guild.Id, options: options).FlattenAsync();
        return subscriptions.GroupBy(x => x.UserId)
            .ToImmutableDictionary(x => RestUser.Create(client, x.First().User) as IUser,
                x => x.GroupBy(y => (y.StartTime, y.EndTime))
                    .Select(y => new BoostSubscriptionMetadata(y.Key.StartTime, y.Key.EndTime, y.Count()))
                    .ToImmutableArray() as IReadOnlyCollection<BoostSubscriptionMetadata>);
    }

    public static async Task<ImmutableDictionary<IUser, IReadOnlyCollection<BoostSubscriptionMetadata>>> GetActiveBoostSubscriptionsAsync(
        IGuild guild, BaseKookClient client, RequestOptions options)
    {
        IEnumerable<BoostSubscription> subscriptions = await client.ApiClient
            .GetGuildBoostSubscriptionsAsync(guild.Id, DateTimeOffset.Now.Add(-KookConfig.BoostPackDuration), options: options).FlattenAsync();
        return subscriptions.GroupBy(x => x.UserId)
            .ToImmutableDictionary(x => RestUser.Create(client, x.First().User) as IUser,
                x => x.GroupBy(y => (y.StartTime, y.EndTime))
                    .Select(y => new BoostSubscriptionMetadata(y.Key.StartTime, y.Key.EndTime, y.Count()))
                    .ToImmutableArray() as IReadOnlyCollection<BoostSubscriptionMetadata>);
    }

    public static int GetMaxBitrate(IGuild guild)
    {
        int tierFactor = guild.BoostLevel switch
        {
            BoostLevel.Level1 => 128,
            BoostLevel.Level2 => 192,
            BoostLevel.Level3 or BoostLevel.Level4 => 256,
            BoostLevel.Level5 or BoostLevel.Level6 => 320,
            _ => 96
        };

        return tierFactor * 1000;
    }

    public static ulong GetUploadLimit(IGuild guild)
    {
        int tierFactor = guild.BoostLevel switch
        {
            BoostLevel.Level1 => 20,
            BoostLevel.Level2 => 50,
            BoostLevel.Level3 => 100,
            BoostLevel.Level4 => 150,
            BoostLevel.Level5 => 200,
            BoostLevel.Level6 => 300,
            _ => 5
        };

        double mebibyte = Math.Pow(2, 20);
        return (ulong)(tierFactor * mebibyte);
    }

    #endregion

    #region Invites

    public static async Task<IReadOnlyCollection<RestInvite>> GetInvitesAsync(IGuild guild, BaseKookClient client,
        RequestOptions options)
    {
        IEnumerable<Invite> models =
            await client.ApiClient.GetGuildInvitesAsync(guild.Id, null, options: options).FlattenAsync().ConfigureAwait(false);
        return models.Select(x => RestInvite.Create(client, guild, x.ChannelId.HasValue
            ? guild.GetChannelAsync(x.ChannelId.Value, CacheMode.CacheOnly).GetAwaiter().GetResult()
            : null, x)).ToImmutableArray();
    }

    /// <exception cref="ArgumentException">
    /// <paramref name="guild.Id"/> may not be equal to zero.
    /// <paramref name="maxAge"/> and <paramref name="maxUses"/> must be greater than zero.
    /// <paramref name="maxAge"/> must be lesser than 604800.
    /// </exception>
    public static async Task<RestInvite> CreateInviteAsync(IGuild guild, BaseKookClient client,
        int? maxAge, int? maxUses, RequestOptions options)
    {
        CreateGuildInviteParams args = new() { GuildId = guild.Id, MaxAge = (InviteMaxAge)(maxAge ?? 0), MaxUses = (InviteMaxUses)(maxUses ?? -1) };
        CreateGuildInviteResponse model = await client.ApiClient.CreateGuildInviteAsync(args, options).ConfigureAwait(false);
        IEnumerable<Invite> invites = await client.ApiClient.GetGuildInvitesAsync(guild.Id, options: options).FlattenAsync().ConfigureAwait(false);
        Invite invite = invites.SingleOrDefault(x => x.Url == model.Url);
        return RestInvite.Create(client, guild, invite?.ChannelId.HasValue ?? false
            ? guild.GetChannelAsync(invite.ChannelId.Value, CacheMode.CacheOnly).GetAwaiter().GetResult()
            : null, invite);
    }

    public static Task<RestInvite> CreateInviteAsync(IGuild channel, BaseKookClient client,
        InviteMaxAge maxAge, InviteMaxUses maxUses, RequestOptions options) =>
        CreateInviteAsync(channel, client, (int?)maxAge, (int?)maxUses, options);

    #endregion

    #region Roles

    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
    public static async Task<RestRole> CreateRoleAsync(IGuild guild, BaseKookClient client,
        string name, RequestOptions options)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));

        CreateGuildRoleParams createGuildRoleParams = new() { Name = name, GuildId = guild.Id };

        Role model = await client.ApiClient.CreateGuildRoleAsync(createGuildRoleParams, options).ConfigureAwait(false);

        return RestRole.Create(client, guild, model);
    }

    #endregion

    #region Users

    public static IAsyncEnumerable<IReadOnlyCollection<RestGuildUser>> GetUsersAsync(IGuild guild, BaseKookClient client, int limit, int fromPage,
        RequestOptions options) =>
        client.ApiClient.GetGuildMembersAsync(guild.Id, limit: limit, fromPage: fromPage, options: options)
            .Select(x => x
                .Select(y => RestGuildUser.Create(client, guild, y)).ToImmutableArray() as IReadOnlyCollection<RestGuildUser>);

    public static async Task<RestGuildUser> GetUserAsync(IGuild guild, BaseKookClient client,
        ulong id, RequestOptions options)
    {
        GuildMember model = await client.ApiClient.GetGuildMemberAsync(guild.Id, id, options).ConfigureAwait(false);
        if (model != null) return RestGuildUser.Create(client, guild, model);

        return null;
    }

    public static async Task<(IReadOnlyCollection<ulong> Muted, IReadOnlyCollection<ulong> Deafened)> GetGuildMutedDeafenedUsersAsync(IGuild guild,
        BaseKookClient client,
        RequestOptions options)
    {
        GetGuildMuteDeafListResponse models = await client.ApiClient.GetGuildMutedDeafenedUsersAsync(guild.Id, options).ConfigureAwait(false);
        return (models.Muted.UserIds, models.Deafened.UserIds);
    }

    public static async Task MuteUserAsync(IGuildUser user, BaseKookClient client, RequestOptions options)
    {
        CreateOrRemoveGuildMuteDeafParams args = new() { GuildId = user.GuildId, UserId = user.Id, Type = MuteOrDeafType.Mute };
        await client.ApiClient.CreateGuildMuteDeafAsync(args, options).ConfigureAwait(false);
    }

    public static async Task DeafenUserAsync(IGuildUser user, BaseKookClient client, RequestOptions options)
    {
        CreateOrRemoveGuildMuteDeafParams args = new() { GuildId = user.GuildId, UserId = user.Id, Type = MuteOrDeafType.Deaf };
        await client.ApiClient.CreateGuildMuteDeafAsync(args, options).ConfigureAwait(false);
    }

    public static async Task UnmuteUserAsync(IGuildUser user, BaseKookClient client, RequestOptions options)
    {
        CreateOrRemoveGuildMuteDeafParams args = new() { GuildId = user.GuildId, UserId = user.Id, Type = MuteOrDeafType.Mute };
        await client.ApiClient.RemoveGuildMuteDeafAsync(args, options).ConfigureAwait(false);
    }

    public static async Task UndeafenUserAsync(IGuildUser user, BaseKookClient client, RequestOptions options)
    {
        CreateOrRemoveGuildMuteDeafParams args = new() { GuildId = user.GuildId, UserId = user.Id, Type = MuteOrDeafType.Deaf };
        await client.ApiClient.RemoveGuildMuteDeafAsync(args, options).ConfigureAwait(false);
    }

    public static IAsyncEnumerable<IReadOnlyCollection<RestGuildUser>> SearchUsersAsync(IGuild guild, BaseKookClient client,
        Action<SearchGuildMemberProperties> func, int limit, RequestOptions options)
    {
        IAsyncEnumerable<IReadOnlyCollection<GuildMember>> models = client.ApiClient.GetGuildMembersAsync(guild.Id, func, limit, 1, options);
        return models.Select(x => x.Select(y => RestGuildUser.Create(client, guild, y)).ToImmutableArray() as IReadOnlyCollection<RestGuildUser>);
    }

    #endregion

    #region Channels

    public static async Task<RestGuildChannel> GetChannelAsync(IGuild guild, BaseKookClient client,
        ulong id, RequestOptions options)
    {
        Channel model = await client.ApiClient.GetGuildChannelAsync(id, options).ConfigureAwait(false);
        if (model != null) return RestGuildChannel.Create(client, guild, model);

        return null;
    }

    public static async Task<IReadOnlyCollection<RestGuildChannel>> GetChannelsAsync(IGuild guild, BaseKookClient client,
        RequestOptions options)
    {
        IEnumerable<Channel> models = await client.ApiClient.GetGuildChannelsAsync(guild.Id, options: options).FlattenAsync().ConfigureAwait(false);
        return models.Select(x => RestGuildChannel.Create(client, guild, x)).ToImmutableArray();
    }

    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
    public static async Task<RestTextChannel> CreateTextChannelAsync(IGuild guild, BaseKookClient client,
        string name, RequestOptions options, Action<CreateTextChannelProperties> func)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));

        CreateTextChannelProperties props = new();
        func?.Invoke(props);

        CreateGuildChannelParams args = new() { Name = name, CategoryId = props.CategoryId, GuildId = guild.Id, Type = ChannelType.Text };
        Channel model = await client.ApiClient.CreateGuildChannelAsync(args, options).ConfigureAwait(false);
        return RestTextChannel.Create(client, guild, model);
    }

    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
    public static async Task<RestVoiceChannel> CreateVoiceChannelAsync(IGuild guild, BaseKookClient client,
        string name, RequestOptions options, Action<CreateVoiceChannelProperties> func)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));

        CreateVoiceChannelProperties props = new();
        func?.Invoke(props);

        CreateGuildChannelParams args = new()
        {
            Name = name,
            CategoryId = props.CategoryId,
            GuildId = guild.Id,
            Type = ChannelType.Voice,
            LimitAmount = props.UserLimit,
            VoiceQuality = props.VoiceQuality
        };
        Channel model = await client.ApiClient.CreateGuildChannelAsync(args, options).ConfigureAwait(false);
        return RestVoiceChannel.Create(client, guild, model);
    }

    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
    public static async Task<RestCategoryChannel> CreateCategoryChannelAsync(IGuild guild, BaseKookClient client,
        string name, RequestOptions options, Action<CreateCategoryChannelProperties> func = null)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));

        CreateCategoryChannelProperties props = new();
        func?.Invoke(props);

        CreateGuildChannelParams args = new() { GuildId = guild.Id, Name = name, IsCategory = 1 };

        Channel model = await client.ApiClient.CreateGuildChannelAsync(args, options).ConfigureAwait(false);
        return RestCategoryChannel.Create(client, guild, model);
    }

    #endregion

    #region Bans

    public static async Task<IReadOnlyCollection<RestBan>> GetBansAsync(IGuild guild, BaseKookClient client,
        RequestOptions options)
    {
        IReadOnlyCollection<Ban> models = await client.ApiClient.GetGuildBansAsync(guild.Id, options).ConfigureAwait(false);
        return models.Select(x => RestBan.Create(client, x)).ToImmutableArray();
    }

    public static async Task<RestBan> GetBanAsync(IGuild guild, BaseKookClient client, ulong userId, RequestOptions options)
    {
        IReadOnlyCollection<Ban> models = await client.ApiClient.GetGuildBansAsync(guild.Id, options).ConfigureAwait(false);
        Ban model = models.FirstOrDefault(x => x.User.Id == userId);
        return model == null ? null : RestBan.Create(client, model);
    }

    public static async Task AddBanAsync(IGuild guild, BaseKookClient client,
        ulong userId, int pruneDays, string reason, RequestOptions options)
    {
        CreateGuildBanParams args = new() { DeleteMessageDays = pruneDays, Reason = reason, GuildId = guild.Id, UserId = userId };
        await client.ApiClient.CreateGuildBanAsync(args, options).ConfigureAwait(false);
    }

    public static async Task RemoveBanAsync(IGuild guild, BaseKookClient client,
        ulong userId, RequestOptions options)
    {
        RemoveGuildBanParams args = new() { GuildId = guild.Id, UserId = userId };
        await client.ApiClient.RemoveGuildBanAsync(args, options).ConfigureAwait(false);
    }

    #endregion

    #region Emotes

    public static async Task<IReadOnlyCollection<GuildEmote>> GetEmotesAsync(IGuild guild, BaseKookClient client, RequestOptions options)
    {
        IEnumerable<API.Emoji> models = await client.ApiClient.GetGuildEmotesAsync(guild.Id, options: options).FlattenAsync().ConfigureAwait(false);
        return models.Select(x => x.ToEntity(guild.Id)).ToImmutableArray();
    }

    public static async Task<GuildEmote> GetEmoteAsync(IGuild guild, BaseKookClient client, string id, RequestOptions options)
    {
        IEnumerable<API.Emoji> emote = await client.ApiClient.GetGuildEmotesAsync(guild.Id, options: options).FlattenAsync().ConfigureAwait(false);
        return emote.FirstOrDefault(x => x.Id == id)?.ToEntity(guild.Id);
    }

    public static async Task<GuildEmote> CreateEmoteAsync(IGuild guild, BaseKookClient client, string name, Image image, RequestOptions options)
    {
        CreateGuildEmoteParams args = new() { Name = name, File = image.Stream, GuildId = guild.Id };

        API.Emoji emote = await client.ApiClient.CreateGuildEmoteAsync(args, options).ConfigureAwait(false);
        return emote.ToEntity(guild.Id);
    }

    /// <exception cref="ArgumentNullException"><paramref name="func"/> is <c>null</c>.</exception>
    public static async Task ModifyEmoteNameAsync(IGuild guild, BaseKookClient client, IEmote emote, Action<string> func,
        RequestOptions options)
    {
        if (func == null) throw new ArgumentNullException(nameof(func));

        string name = emote.Name;
        func(name);

        ModifyGuildEmoteParams args = new() { Name = name, Id = emote.Id };
        await client.ApiClient.ModifyGuildEmoteAsync(args, options).ConfigureAwait(false);
    }

    public static async Task DeleteEmoteAsync(IGuild guild, BaseKookClient client, string id, RequestOptions options)
        => await client.ApiClient.DeleteGuildEmoteAsync(id, options).ConfigureAwait(false);

    #endregion

    #region Badges

    public static async Task<Stream> GetBadgeAsync(IGuild guild, BaseKookClient client, BadgeStyle style, RequestOptions options) =>
        await client.ApiClient.GetGuildBadgeAsync(guild.Id, style, options).ConfigureAwait(false);

    #endregion
}
