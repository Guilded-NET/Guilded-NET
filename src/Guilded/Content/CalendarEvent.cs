using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Guilded.Base;
using Guilded.Base.Json;
using Guilded.Client;
using Guilded.Events;
using Guilded.Servers;
using Guilded.Users;
using Newtonsoft.Json;
using SystemColor = System.Drawing.Color;

namespace Guilded.Content;

/// <summary>
/// Represents a calendar event in <see cref="ChannelType.Calendar">a calendar channel</see>.
/// </summary>
/// <seealso cref="CalendarCancellation" />
/// <seealso cref="CalendarRsvp" />
/// <seealso cref="Topic" />
/// <seealso cref="Doc" />
/// <seealso cref="ItemBase{T}" />
/// <seealso cref="Message" />
public class CalendarEvent : ChannelContent<uint, HashId>, IReactibleContent, IServerBased, IPrivatableContent
{
    #region Properties Content
    /// <summary>
    /// Gets the title of the <see cref="CalendarEvent">calendar event</see>.
    /// </summary>
    /// <remarks>
    /// <para>This does not have any Markdown formatting and will not contain <c>\n</c> or other line breaking characters.</para>
    /// </remarks>
    /// <value>Single-line string</value>
    /// <seealso cref="CalendarEvent" />
    /// <seealso cref="Description" />
    /// <seealso cref="Location" />
    public string Name { get; }

    /// <summary>
    /// Gets the description of the <see cref="CalendarEvent">calendar event</see>.
    /// </summary>
    /// <remarks>
    /// <para>The contents are formatted in Markdown. This includes images and videos, which are in the format of <c>![](source_url)</c>.</para>
    /// </remarks>
    /// <value>Markdown string</value>
    /// <seealso cref="CalendarEvent" />
    /// <seealso cref="Name" />
    /// <seealso cref="Location" />
    public string? Description { get; }

    /// <summary>
    /// Gets <see cref="Content.Mentions">the mentions</see> found in the <see cref="Description">description</see>.
    /// </summary>
    /// <value><see cref="Content.Mentions" />?</value>
    public Mentions? Mentions { get; }

    /// <summary>
    /// Gets the physical or non-physical location of the <see cref="CalendarEvent">calendar event</see>.
    /// </summary>
    /// <value>Single-line string</value>
    /// <seealso cref="CalendarEvent" />
    /// <seealso cref="Name" />
    /// <seealso cref="Description" />
    /// <seealso cref="Url" />
    /// <seealso cref="Color" />
    public string? Location { get; }

    /// <summary>
    /// Gets the URL to the <see cref="CalendarEvent">calendar event's</see> services, place or anything related.
    /// </summary>
    /// <value><see cref="Uri">URL</see>?</value>
    /// <seealso cref="CalendarEvent" />
    /// <seealso cref="Name" />
    /// <seealso cref="Description" />
    /// <seealso cref="Location" />
    /// <seealso cref="Color" />
    public Uri? Url { get; }

    /// <summary>
    /// Gets the colour of the <see cref="CalendarEvent">calendar event</see>.
    /// </summary>
    /// <value><see cref="SystemColor">Colour</see>?</value>
    /// <seealso cref="CalendarEvent" />
    /// <seealso cref="Url" />
    /// <seealso cref="Location" />
    /// <seealso cref="Name" />
    /// <seealso cref="Description" />
    [JsonConverter(typeof(DecimalColorConverter))]
    public SystemColor? Color { get; }
    #endregion

    #region Properties Time
    /// <summary>
    /// Gets the date when the <see cref="CalendarEvent">calendar event</see> starts.
    /// </summary>
    /// <value>Date</value>
    /// <seealso cref="CalendarEvent" />
    /// <seealso cref="ChannelContent{T, S}.CreatedAt" />
    /// <seealso cref="ChannelContent{T, S}.CreatedBy" />
    public DateTime StartsAt { get; }

    /// <summary>
    /// Gets the duration of the <see cref="CalendarEvent">calendar event</see> in minutes.
    /// </summary>
    /// <value><see cref="TimeSpan">Duration</see> in minutes</value>
    /// <seealso cref="CalendarEvent" />
    /// <seealso cref="ChannelContent{T, S}.CreatedAt" />
    /// <seealso cref="ChannelContent{T, S}.CreatedBy" />
    public TimeSpan? Duration { get; }

    /// <summary>
    /// Gets the date when the <see cref="CalendarEvent">calendar event</see> starts.
    /// </summary>
    /// <value>Date</value>
    /// <seealso cref="CalendarEvent" />
    /// <seealso cref="ChannelContent{T, S}.CreatedAt" />
    /// <seealso cref="ChannelContent{T, S}.CreatedBy" />
    public DateTime? EndsAt => Duration is null ? null : (StartsAt + Duration);
    #endregion

    #region Properties
    /// <summary>
    /// Gets whether the <see cref="CalendarEvent">calendar event</see> was set as private.
    /// </summary>
    /// <value><see cref="CalendarEvent">Calendar event</see> is private</value>
    /// <seealso cref="CalendarEvent" />
    /// <seealso cref="IsCanceled" />
    public bool IsPrivate { get; }

    /// <summary>
    /// Gets the limit of how many <see cref="User">users</see> can join the <see cref="CalendarEvent">calendar event</see>.
    /// </summary>
    /// <value><see cref="CalendarRsvp">RSVP</see> limit?</value>
    /// <seealso cref="CalendarEvent" />
    /// <seealso cref="CalendarRsvp" />
    public uint? RsvpLimit { get; }

    /// <summary>
    /// Gets the information about the <see cref="CalendarEvent">calendar event's</see> cancellation.
    /// </summary>
    /// <value><see cref="CalendarEvent">Calendar event</see>'s <see cref="CalendarCancellation">cancellation info</see></value>
    public CalendarCancellation? Cancellation { get; }

    /// <summary>
    /// Gets whether the <see cref="CalendarEvent">calendar event</see> was cancelled.
    /// </summary>
    /// <value><see cref="CalendarEvent">Calendar event</see> is private</value>
    /// <seealso cref="CalendarEvent" />
    /// <seealso cref="Cancellation" />
    /// <seealso cref="IsPrivate" />
    public bool IsCanceled => Cancellation is not null;

    /// <inheritdoc cref="CalendarCancellation.CreatedBy" />
    public HashId? CanceledBy => Cancellation?.CreatedBy;
    #endregion

    #region Properties Events
    /// <summary>
    /// Gets the <see cref="IObservable{T}">observable</see> for an event when the <see cref="CalendarEvent">calendar event</see> gets edited.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="IObservable{T}">observable</see> will be filtered for this <see cref="CalendarEvent">calendar event</see> specific.</para>
    /// </remarks>
    /// <returns>The <see cref="IObservable{T}">observable</see> for an event when the <see cref="CalendarEvent">calendar event</see> gets edited</returns>
    /// <seealso cref="Deleted" />
    public IObservable<CalendarEventEvent> Updated =>
        ParentClient
            .EventUpdated
            .Where(x =>
                x.ChannelId == ChannelId && x.Event.Id == Id
            );

    /// <summary>
    /// Gets the <see cref="IObservable{T}">observable</see> for an event when the <see cref="CalendarEvent">calendar event</see> gets deleted.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="IObservable{T}">observable</see> will be filtered for this <see cref="CalendarEvent">calendar event</see> specific.</para>
    /// </remarks>
    /// <returns>The <see cref="IObservable{T}">observable</see> for an event when the <see cref="CalendarEvent">calendar event</see> gets deleted</returns>
    /// <seealso cref="Updated" />
    public IObservable<CalendarEventEvent> Deleted =>
        ParentClient
            .EventDeleted
            .Where(x =>
                x.ChannelId == ChannelId && x.Event.Id == Id
            )
            .Take(1);

    /// <summary>
    /// Gets the <see cref="IObservable{T}">observable</see> for an event when <see cref="CalendarEvent">calendar event's</see> <see cref="CalendarRsvp">RSVP</see> gets added/edited.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="IObservable{T}">observable</see> will be filtered for this <see cref="CalendarEvent">calendar event</see> specific.</para>
    /// </remarks>
    /// <returns>The <see cref="IObservable{T}">observable</see> for an event when the <see cref="CalendarEvent">calendar event's</see> <see cref="CalendarRsvp">RSVP</see> gets added/edited</returns>
    /// <seealso cref="Updated" />
    /// <seealso cref="Deleted" />
    /// <seealso cref="RsvpDeleted" />
    /// <seealso cref="RsvpManyUpdated" />
    public IObservable<CalendarRsvpEvent> RsvpUpdated =>
        ParentClient
            .RsvpUpdated
            .Where(x =>
                x.ChannelId == ChannelId && x.EventId == Id
            );

    /// <summary>
    /// Gets the <see cref="IObservable{T}">observable</see> for an event when the <see cref="CalendarEvent">calendar event's</see> <see cref="CalendarRsvp">RSVP</see> gets deleted.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="IObservable{T}">observable</see> will be filtered for this <see cref="CalendarEvent">calendar event</see> specific.</para>
    /// </remarks>
    /// <returns>The <see cref="IObservable{T}">observable</see> for an event when the <see cref="CalendarEvent">calendar event's</see> <see cref="CalendarRsvp">RSVP</see> gets deleted</returns>
    /// <seealso cref="Updated" />
    /// <seealso cref="Deleted" />
    /// <seealso cref="RsvpUpdated" />
    /// <seealso cref="RsvpManyUpdated" />
    public IObservable<CalendarRsvpEvent> RsvpDeleted =>
        ParentClient
            .RsvpDeleted
            .Where(x =>
                x.ChannelId == ChannelId && x.EventId == Id
            );

    /// <summary>
    /// Gets the <see cref="IObservable{T}">observable</see> for an event when the <see cref="CalendarEvent">calendar event's</see> multiple <see cref="CalendarRsvp">RSVPs</see> gets added/edited.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="IObservable{T}">observable</see> will be filtered for this <see cref="CalendarEvent">calendar event</see> specific.</para>
    /// </remarks>
    /// <returns>The <see cref="IObservable{T}">observable</see> for an event when the <see cref="CalendarEvent">calendar event's</see> multiple <see cref="CalendarRsvp">RSVPs</see> gets added/edited</returns>
    /// <seealso cref="Updated" />
    /// <seealso cref="Deleted" />
    /// <seealso cref="RsvpUpdated" />
    /// <seealso cref="RsvpDeleted" />
    public IObservable<CalendarRsvpManyEvent> RsvpManyUpdated =>
        ParentClient
            .RsvpManyUpdated
            .Where(x =>
                x.ChannelId == ChannelId && x.EventId == Id
            );
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of <see cref="CalendarEvent" /> from the specified JSON properties.
    /// </summary>
    /// <param name="id">The identifier of the channel content</param>
    /// <param name="channelId">The identifier of the channel where the channel content are</param>
    /// <param name="serverId">The identifier of the <see cref="Server">server</see> where the channel content are</param>
    /// <param name="name">The title of the <see cref="CalendarEvent">calendar event</see></param>
    /// <param name="description">The description of the <see cref="CalendarEvent">calendar event</see></param>
    /// <param name="mentions"><see cref="Content.Mentions">The mentions</see> found in the <see cref="Description">description</see></param>
    /// <param name="location">The physical or non-physical location of the <see cref="CalendarEvent">calendar event</see></param>
    /// <param name="url">The URL to the <see cref="CalendarEvent">calendar event's</see> services, place or anything related</param>
    /// <param name="color">The colour of the <see cref="CalendarEvent">calendar event</see></param>
    /// <param name="duration">The duration of the <see cref="CalendarEvent">calendar event</see> in minutes</param>
    /// <param name="isPrivate">Whether the <see cref="CalendarEvent">calendar event</see> was set as private</param>
    /// <param name="cancellation">The information about the <see cref="CalendarEvent">calendar event's</see> cancellation</param>
    /// <param name="createdBy">The identifier of <see cref="User">user</see> that created the <see cref="CalendarEvent">calendar event</see></param>
    /// <param name="createdAt">The date when the <see cref="CalendarEvent">calendar event</see> were created</param>
    /// <param name="startsAt">The date when the <see cref="CalendarEvent">calendar event</see> starts</param>
    /// <returns>New <see cref="CalendarEvent" /> JSON instance</returns>
    /// <seealso cref="CalendarEvent" />
    [JsonConstructor]
    public CalendarEvent(
        [JsonProperty(Required = Required.Always)]
        uint id,

        [JsonProperty(Required = Required.Always)]
        Guid channelId,

        [JsonProperty(Required = Required.Always)]
        HashId serverId,

        [JsonProperty(Required = Required.Always)]
        string name,

        [JsonProperty(Required = Required.Always)]
        HashId createdBy,

        [JsonProperty(Required = Required.Always)]
        DateTime createdAt,

        [JsonProperty(Required = Required.Always)]
        DateTime startsAt,

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        string? description = null,

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        Mentions? mentions = null,

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        string? location = null,

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        Uri? url = null,

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        SystemColor? color = null,

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        uint? duration = null,

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        bool isPrivate = false,

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        CalendarCancellation? cancellation = null
    ) : base(id, channelId, serverId, createdBy, createdAt) =>
        (Name, Description, Mentions, Location, Url, Color, StartsAt, Duration, IsPrivate, Cancellation) = (name, description, mentions, location, url, color, startsAt, duration is not null ? TimeSpan.FromMinutes((double)duration) : null, isPrivate, cancellation);
    #endregion

    #region Methods Event
    /// <inheritdoc cref="AbstractGuildedClient.UpdateEventAsync(Guid, uint, string?, string?, string?, DateTime?, Uri?, SystemColor?, uint?, bool?)" />
    public Task<CalendarEvent> UpdateAsync(string? name = null, string? description = null, string? location = null, DateTime? startsAt = null, Uri? url = null, SystemColor? color = null, uint? duration = null, bool? isPrivate = null) =>
        ParentClient.UpdateEventAsync(ChannelId, Id, name, description, location, startsAt, url, color, duration, isPrivate);

    /// <inheritdoc cref="AbstractGuildedClient.UpdateEventAsync(Guid, uint, string?, string?, string?, DateTime?, Uri?, SystemColor?, TimeSpan?, bool?)" />
    public Task<CalendarEvent> UpdateAsync(string? name = null, string? description = null, string? location = null, DateTime? startsAt = null, Uri? url = null, SystemColor? color = null, TimeSpan? duration = null, bool? isPrivate = null) =>
        ParentClient.UpdateEventAsync(ChannelId, Id, name, description, location, startsAt, url, color, duration, isPrivate);

    /// <inheritdoc cref="AbstractGuildedClient.DeleteEventAsync(Guid, uint)" />
    public Task DeleteAsync() =>
        ParentClient.DeleteEventAsync(ChannelId, Id);

    /// <inheritdoc cref="AbstractGuildedClient.AddReactionAsync(Guid, uint, uint)" />
    /// <param name="emoteId">The identifier of the <see cref="Emote">emote</see> to add</param>
    public Task AddReactionAsync(uint emoteId) =>
        ParentClient.AddReactionAsync(ChannelId, Id, emoteId);

    /// <inheritdoc cref="AbstractGuildedClient.RemoveReactionAsync(Guid, uint, uint)" />
    /// <param name="emoteId">The identifier of the <see cref="Emote">emote</see> to remove</param>
    public Task RemoveReactionAsync(uint emoteId) =>
        ParentClient.RemoveReactionAsync(ChannelId, Id, emoteId);
    #endregion

    #region Methods RSVP
    /// <inheritdoc cref="AbstractGuildedClient.GetRsvpsAsync(Guid, uint)" />
    public Task<IList<CalendarRsvp>> GetRsvpsAsync() =>
        ParentClient.GetRsvpsAsync(ChannelId, Id);

    /// <inheritdoc cref="AbstractGuildedClient.GetRsvpAsync(Guid, uint, HashId)" />
    /// <param name="user">The identifier of the <see cref="User">user</see> to get <see cref="CalendarRsvp">RSVP</see> of</param>
    public Task<CalendarRsvp> GetRsvpAsync(HashId user) =>
        ParentClient.GetRsvpAsync(ChannelId, Id, user);

    /// <inheritdoc cref="AbstractGuildedClient.SetRsvpAsync(Guid, uint, HashId, CalendarRsvpStatus)" />
    /// <param name="user">The identifier of the <see cref="User">user</see> to set <see cref="CalendarRsvp">RSVP</see> of</param>
    /// <param name="status">The status of the <see cref="CalendarEvent">calendar RSVP</see> to set</param>
    public Task<CalendarRsvp> SetRsvpAsync(HashId user, CalendarRsvpStatus status) =>
        ParentClient.SetRsvpAsync(ChannelId, Id, user, status);

    /// <inheritdoc cref="AbstractGuildedClient.RemoveRsvpAsync(Guid, uint, HashId)" />
    /// <param name="user">The identifier of the <see cref="User">user</see> to remove <see cref="CalendarRsvp">RSVP</see> of</param>
    public Task RemoveRsvpAsync(HashId user) =>
        ParentClient.RemoveRsvpAsync(ChannelId, Id, user);
    #endregion
}

/// <summary>
/// Represents the cancellation of a <see cref="CalendarEvent">calendar event</see>.
/// </summary>
/// <seealso cref="CalendarEvent" />
/// <seealso cref="ItemNote" />
/// <seealso cref="ItemNoteSummary" />
public class CalendarCancellation
{
    #region Properties
    /// <summary>
    /// Gets the reason why the <see cref="CalendarEvent">calendar event</see> was cancelled.
    /// </summary>
    /// <value>String</value>
    /// <seealso cref="CalendarCancellation" />
    /// <seealso cref="CalendarEvent" />
    /// <seealso cref="CreatedBy" />
    public string Description { get; }

    /// <summary>
    /// Gets the identifier of <see cref="User">user</see> that cancelled the <see cref="CalendarEvent">calendar event</see>.
    /// </summary>
    /// <value><see cref="UserSummary.Id">User ID</see></value>
    /// <seealso cref="CalendarCancellation" />
    /// <seealso cref="CalendarEvent" />
    /// <seealso cref="Description" />
    /// <seealso cref="ChannelContent{TId, TServer}.CreatedBy" />
    public HashId CreatedBy { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of <see cref="CalendarCancellation" /> from the specified JSON properties.
    /// </summary>
    /// <param name="description">The reason why the <see cref="CalendarEvent">calendar event</see> was cancelled</param>
    /// <param name="createdBy">The identifier of <see cref="User">user</see> that cancelled the <see cref="CalendarEvent">calendar event</see></param>
    /// <returns>New <see cref="CalendarCancellation" /> JSON instance</returns>
    /// <seealso cref="CalendarCancellation" />
    /// <seealso cref="CalendarEvent" />
    [JsonConstructor]
    public CalendarCancellation(
        [JsonProperty(Required = Required.Always)]
        string description,

        [JsonProperty(Required = Required.Always)]
        HashId createdBy
    ) =>
        (Description, CreatedBy) = (description, createdBy);
    #endregion
}