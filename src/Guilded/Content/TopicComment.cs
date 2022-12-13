using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Guilded.Base;
using Guilded.Client;
using Guilded.Events;
using Guilded.Servers;
using Guilded.Users;
using Newtonsoft.Json;

namespace Guilded.Content;

/// <summary>
/// Represents a reply in a <see cref="Topic">forum topic</see>.
/// </summary>
/// <seealso cref="Topic" />
/// <seealso cref="ForumChannel" />
/// <seealso cref="TitledContent" />
public class TopicComment : ContentModel, IModelHasId<uint>, ICreatableContent, IUpdatableContent, IContentMarkdown
{
    #region Properties
    /// <summary>
    /// Gets the identifier of the <see cref="TopicComment">forum topic reply</see>.
    /// </summary>
    /// <value>The identifier of the <see cref="TopicComment">forum topic reply</see></value>
    /// <seealso cref="TopicComment" />
    /// <seealso cref="TopicId" />
    /// <seealso cref="ChannelId" />
    /// <seealso cref="CreatedBy" />
    public uint Id { get; }

    /// <summary>
    /// Gets the identifier of the <see cref="Topic">forum topic</see> where the <see cref="TopicComment">forum topic reply</see> was created.
    /// </summary>
    /// <value>The identifier of the <see cref="Topic">forum topic</see> where the <see cref="TopicComment">forum topic reply</see> was created</value>
    /// <seealso cref="TopicComment" />
    /// <seealso cref="Id" />
    /// <seealso cref="ChannelId" />
    /// <seealso cref="CreatedBy" />
    public uint TopicId { get; }

    /// <summary>
    /// Gets the identifier of the <see cref="ForumChannel">forum channel</see> where the <see cref="TopicComment">forum topic reply</see> was created.
    /// </summary>
    /// <value>The identifier of the <see cref="ForumChannel">forum channel</see> where the <see cref="TopicComment">forum topic reply</see> was created</value>
    /// <seealso cref="TopicComment" />
    /// <seealso cref="Id" />
    /// <seealso cref="TopicId" />
    /// <seealso cref="CreatedBy" />
    public Guid ChannelId { get; }

    /// <summary>
    /// Gets the full-Markdown text contents of the <see cref="TopicComment">forum topic reply</see>.
    /// </summary>
    /// <remarks>
    /// <para>The contents are formatted in Markdown. This includes images and videos, which are in the format of <c>![](source_url)</c>.</para>
    /// </remarks>
    /// <value>A multi-line text formatted in extended Guilded Markdown</value>
    /// <seealso cref="TopicComment" />
    /// <seealso cref="Mentions" />
    /// <seealso cref="CreatedBy" />
    /// <seealso cref="CreatedAt" />
    /// <seealso cref="Id" />
    /// <seealso cref="TopicId" />
    public string Content { get; }

    /// <summary>
    /// Gets the <see cref="Content.Mentions">mentions</see> found in the <see cref="Content">content</see>.
    /// </summary>
    /// <value><see cref="Content.Mentions" />?</value>
    /// <seealso cref="TopicComment" />
    /// <seealso cref="Content" />
    /// <seealso cref="CreatedBy" />
    /// <seealso cref="CreatedAt" />
    /// <seealso cref="Id" />
    /// <seealso cref="TopicId" />
    public Mentions? Mentions { get; }

    /// <summary>
    /// Gets the identifier of <see cref="User">user</see> that created the <see cref="TopicComment">forum topic</see>.
    /// </summary>
    /// <remarks>
    /// <para>If webhook or bot created this reaction, the value of this property will be <c>Ann6LewA</c>.</para>
    /// </remarks>
    /// <value>The <see cref="User">creator</see> identifier of the <see cref="TopicComment">forum topic</see></value>
    /// <seealso cref="TopicComment" />
    /// <seealso cref="CreatedAt" />
    /// <seealso cref="UpdatedAt" />
    /// <seealso cref="Content" />
    public HashId CreatedBy { get; }

    /// <summary>
    /// Gets the date when the <see cref="TopicComment">forum topic reply</see> was created.
    /// </summary>
    /// <value>The creation date of the <see cref="TopicComment">forum topic reply</see></value>
    /// <seealso cref="TopicComment" />
    /// <seealso cref="CreatedBy" />
    /// <seealso cref="UpdatedAt" />
    /// <seealso cref="Content" />
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Gets the date when the <see cref="TopicComment">forum topic comment</see> was edited.
    /// </summary>
    /// <remarks>
    /// <para>Only returns the most recent update.</para>
    /// </remarks>
    /// <value>The edit date of the <see cref="TopicComment">forum topic comment</see></value>
    /// <seealso cref="TopicComment" />
    /// <seealso cref="CreatedAt" />
    /// <seealso cref="CreatedBy" />
    /// <seealso cref="Content" />
    public DateTime? UpdatedAt { get; }
    #endregion

    #region Properties Events
    /// <summary>
    /// Gets the <see cref="IObservable{T}">observable</see> for an event when the <see cref="TopicComment">forum topic comment</see> gets edited.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="IObservable{T}">observable</see> will be filtered for this <see cref="TopicComment">forum topic comment</see> specific.</para>
    /// </remarks>
    /// <returns>The <see cref="IObservable{T}">observable</see> for an event when the <see cref="TopicComment">forum topic comment</see> gets edited</returns>
    /// <seealso cref="Deleted" />
    public IObservable<TopicCommentEvent> Updated =>
        ParentClient
            .TopicCommentUpdated
            .Where(x =>
                x.ChannelId == ChannelId && x.TopicId == Id && x.Id == TopicId
            );

    /// <summary>
    /// Gets the <see cref="IObservable{T}">observable</see> for an event when the <see cref="Topic">forum topic</see> gets deleted.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="IObservable{T}">observable</see> will be filtered for this <see cref="Topic">forum topic</see> specific.</para>
    /// </remarks>
    /// <returns>The <see cref="IObservable{T}">observable</see> for an event when the <see cref="Topic">forum topic</see> gets deleted</returns>
    /// <seealso cref="Updated" />
    public IObservable<TopicCommentEvent> Deleted =>
        ParentClient
            .TopicCommentDeleted
            .Where(x =>
                x.ChannelId == ChannelId && x.TopicId == Id && x.Id == TopicId
            )
            .Take(1);
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of <see cref="TopicComment" /> from the specified JSON properties.
    /// </summary>
    /// <param name="id">The identifier of the <see cref="TopicComment">forum topic reply</see></param>
    /// <param name="forumTopicId">The identifier of the <see cref="Topic">forum topic</see> where the <see cref="TopicComment">forum topic reply</see> was created</param>
    /// <param name="channelId">The identifier of the <see cref="ServerChannel">channel</see> where the <see cref="TopicComment">forum topic reply</see> was created</param>
    /// <param name="content">The full-Markdown text contents of the <see cref="TopicComment">forum topic reply</see></param>
    /// <param name="createdBy">The identifier of <see cref="User">user</see> that created the <see cref="TopicComment">forum topic</see></param>
    /// <param name="createdAt">The date when the <see cref="TopicComment">forum topic reply</see> was created</param>
    /// <param name="updatedAt">The date when the <see cref="TopicComment">forum topic comment</see> was edited</param>
    /// <returns>New <see cref="TopicComment" /> JSON instance</returns>
    /// <seealso cref="TopicComment" />
    [JsonConstructor]
    public TopicComment(
        [JsonProperty(Required = Required.Always)]
        uint id,

        [JsonProperty(Required = Required.Always)]
        uint forumTopicId,

        [JsonProperty(Required = Required.Always)]
        Guid channelId,

        [JsonProperty(Required = Required.Always)]
        string content,

        [JsonProperty(Required = Required.Always)]
        HashId createdBy,

        [JsonProperty(Required = Required.Always)]
        DateTime createdAt,

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        DateTime? updatedAt = null
    ) =>
        (Id, ChannelId, TopicId, Content, CreatedBy, CreatedAt, UpdatedAt) = (id, channelId, forumTopicId, content, createdBy, createdAt, updatedAt);
    #endregion

    #region Methods
    /// <inheritdoc cref="AbstractGuildedClient.CreateTopicCommentAsync(Guid, uint, string)" />
    /// <param name="content">The content of the <see cref="TopicComment">forum topic comment</see></param>
    public Task<TopicComment> ReplyAsync(string content) =>
        ParentClient.CreateTopicCommentAsync(ChannelId, TopicId, content);

    /// <inheritdoc cref="AbstractGuildedClient.UpdateTopicCommentAsync(Guid, uint, uint, string)" />
    /// <param name="content">The new Markdown content of the <see cref="TopicComment">forum topic comment</see></param>
    public Task<TopicComment> UpdateAsync(string content) =>
        ParentClient.UpdateTopicCommentAsync(ChannelId, TopicId, Id, content);

    /// <inheritdoc cref="AbstractGuildedClient.DeleteTopicCommentAsync(Guid, uint, uint)" />
    public Task DeleteAsync() =>
        ParentClient.DeleteTopicCommentAsync(ChannelId, TopicId, Id);
    #endregion
}