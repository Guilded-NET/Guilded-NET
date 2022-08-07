using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Guilded.Base;
using Guilded.Base.Servers;
using RestSharp;

namespace Guilded;

public abstract partial class AbstractGuildedClient
{
    #region Methods

    #region Methods Servers specifically
    /// <inheritdoc />
    public override Task<Server> GetServerAsync(HashId server) =>
        GetResponseProperty<Server>(new RestRequest($"servers/{server}", Method.Get), "server");
    #endregion

    #region Methods Groups
    /// <inheritdoc />
    public override Task AddMembershipAsync(HashId group, HashId member) =>
        ExecuteRequestAsync(new RestRequest($"groups/{group}/members/{member}", Method.Put));

    /// <inheritdoc />
    public override Task RemoveMembershipAsync(HashId group, HashId member) =>
        ExecuteRequestAsync(new RestRequest($"groups/{group}/members/{member}", Method.Delete));
    #endregion

    #region Methods Members
    /// <inheritdoc />
    public override Task<IList<MemberSummary>> GetMembersAsync(HashId server) =>
        GetResponseProperty<IList<MemberSummary>>(new RestRequest($"servers/{server}/members", Method.Get), "members");

    /// <inheritdoc />
    public override Task<Member> GetMemberAsync(HashId server, HashId member) =>
        GetResponseProperty<Member>(new RestRequest($"servers/{server}/members/{member}", Method.Get), "member");

    /// <inheritdoc />
    public override Task<IList<uint>> GetMemberRolesAsync(HashId server, HashId member) =>
        GetResponseProperty<IList<uint>>(new RestRequest($"servers/{server}/members/{member}/roles", Method.Get), "roleIds");

    /// <inheritdoc />
    public override Task<string> UpdateNicknameAsync(HashId server, HashId member, string nickname)
    {
        if (string.IsNullOrWhiteSpace(nickname))
            throw new ArgumentNullException(nameof(nickname));
        else if (nickname.Length > 32)
            throw new ArgumentOutOfRangeException(nameof(nickname), nickname, $"Argument {nameof(nickname)} must be 32 characters in length max");

        return GetResponseProperty<string>(new RestRequest($"servers/{server}/members/{member}/nickname", Method.Put)
            .AddJsonBody(new
            {
                nickname
            })
        , "nickname");
    }

    /// <inheritdoc />
    public override Task DeleteNicknameAsync(HashId server, HashId member) =>
        ExecuteRequestAsync(new RestRequest($"servers/{server}/members/{member}/nickname", Method.Delete));

    /// <inheritdoc />
    public override Task AddRoleAsync(HashId server, HashId member, uint role) =>
        ExecuteRequestAsync(new RestRequest($"servers/{server}/members/{member}/roles/{role}", Method.Put));

    /// <inheritdoc />
    public override Task RemoveRoleAsync(HashId server, HashId member, uint role) =>
        ExecuteRequestAsync(new RestRequest($"servers/{server}/members/{member}/roles/{role}", Method.Delete));

    /// <inheritdoc />
    public override Task<long> AddXpAsync(HashId server, HashId member, short amount) =>
        GetResponseProperty<long>(new RestRequest($"servers/{server}/members/{member}/xp", Method.Post)
            .AddJsonBody(new
            {
                amount
            })
        , "total");

    /// <inheritdoc />
    public override Task AddXpAsync(HashId server, uint role, short amount) =>
        ExecuteRequestAsync(new RestRequest($"servers/{server}/roles/{role}/xp", Method.Post)
            .AddJsonBody(new
            {
                amount
            })
        );
    #endregion

    #region Methods Server-wide Moderation
    /// <inheritdoc />
    public override Task RemoveMemberAsync(HashId server, HashId member) =>
        ExecuteRequestAsync(new RestRequest($"servers/{server}/members/{member}", Method.Delete));

    /// <inheritdoc />
    public override Task<IList<MemberBan>> GetBansAsync(HashId server) =>
        GetResponseProperty<IList<MemberBan>>(new RestRequest($"servers/{server}/bans", Method.Get), "serverMemberBans");

    /// <inheritdoc />
    public override Task<MemberBan> GetBanAsync(HashId server, HashId member) =>
        GetResponseProperty<MemberBan>(new RestRequest($"servers/{server}/bans/{member}", Method.Get), "serverMemberBan");

    /// <inheritdoc />
    public override Task<MemberBan> AddMemberBanAsync(HashId server, HashId member, string? reason = null) =>
        GetResponseProperty<MemberBan>(new RestRequest($"servers/{server}/bans/{member}", Method.Post)
            .AddJsonBody(new
            {
                reason
            })
        , "serverMemberBan");

    /// <inheritdoc />
    public override Task RemoveMemberBanAsync(HashId server, HashId member) =>
        ExecuteRequestAsync(new RestRequest($"servers/{server}/bans/{member}", Method.Delete));
    #endregion

    #region Methods Webhooks
    /// <inheritdoc />
    public override Task<IList<Webhook>> GetWebhooksAsync(HashId server, Guid? channel = null) =>
        GetResponseProperty<IList<Webhook>>(new RestRequest($"servers/{server}/webhooks", Method.Get)
            .AddOptionalQuery("channelId", channel)
        , "webhooks");

    /// <inheritdoc />
    public override Task<Webhook> GetWebhookAsync(HashId server, Guid webhook) =>
        GetResponseProperty<Webhook>(new RestRequest($"servers/{server}/webhooks/{webhook}", Method.Get), "webhook");

    /// <inheritdoc />
    public override Task<Webhook> CreateWebhookAsync(HashId server, Guid channel, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        return GetResponseProperty<Webhook>(new RestRequest($"servers/{server}/webhooks", Method.Post)
            .AddJsonBody(new
            {
                name,
                channelId = channel
            })
        , "webhook");
    }

    /// <inheritdoc />
    public override Task<Webhook> UpdateWebhookAsync(HashId server, Guid webhook, string name, Guid? newChannel = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        return GetResponseProperty<Webhook>(new RestRequest($"servers/{server}/webhooks/{webhook}", Method.Put)
            .AddJsonBody(new
            {
                name,
                channelId = newChannel
            })
        , "webhook");
    }

    /// <inheritdoc />
    public override Task DeleteWebhookAsync(HashId server, Guid webhook) =>
        ExecuteRequestAsync(new RestRequest($"servers/{server}/webhooks/{webhook}", Method.Delete));
    #endregion

    #region Methods Channels
    /// <inheritdoc />
    public override Task<ServerChannel> GetChannelAsync(Guid channel) =>
        GetResponseProperty<ServerChannel>(new RestRequest($"channels/{channel}", Method.Get), "channel");

    /// <inheritdoc />
    public override Task<ServerChannel> CreateChannelAsync(HashId server, string name, ChannelType type = ChannelType.Chat, string? topic = null, HashId? group = null, uint? category = null, bool? isPublic = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        EnforceLimit(nameof(name), name, ServerChannel.NameLimit);
        EnforceLimitOnNullable(nameof(topic), topic, ServerChannel.TopicLimit);

        return GetResponseProperty<ServerChannel>(new RestRequest($"channels", Method.Post)
            .AddJsonBody(new
            {
                serverId = server,
                groupId = group,
                categoryId = category,
                name,
                type,
                topic,
                isPublic
            })
        , "channel");
    }

    /// <inheritdoc />
    public override Task<ServerChannel> UpdateChannelAsync(Guid channel, string? name = null, string? topic = null, bool? isPublic = null)
    {
        return GetResponseProperty<ServerChannel>(new RestRequest($"channels/{channel}", Method.Patch)
            .AddJsonBody(new
            {
                name,
                topic,
                isPublic
            })
        , "channel");
    }

    /// <inheritdoc />
    public override Task DeleteChannelAsync(Guid channel) =>
        ExecuteRequestAsync(new RestRequest($"channels/{channel}", Method.Delete));
    #endregion

    #endregion
}