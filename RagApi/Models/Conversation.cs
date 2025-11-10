using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RagApi.Models;

/// <summary>
/// Represents a conversation between a user and the RAG system
/// </summary>
public class Conversation
{
    /// <summary>
    /// Unique identifier for the conversation
    /// </summary>
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Title of the conversation
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// When the conversation was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the conversation was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Messages in this conversation
    /// </summary>
    [InverseProperty("Conversation")]
    public List<Message> Messages { get; set; } = new List<Message>();

    // UserId is implemented as a shadow property in DbContext
    // This allows us to have the relationship without exposing it directly in the model
}

/// <summary>
/// Represents a message in a conversation
/// </summary>
public class Message
{
    /// <summary>
    /// Unique identifier for the message
    /// </summary>
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the conversation this message belongs to
    /// </summary>
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the conversation
    /// </summary>
    [ForeignKey("ConversationId")]
    public Conversation? Conversation { get; set; }

    /// <summary>
    /// Type of the message (user or system)
    /// </summary>
    public MessageType Type { get; set; }

    /// <summary>
    /// Content of the message
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// JSON string containing search results related to this message
    /// </summary>
    public string? SearchResultsJson { get; set; }

    /// <summary>
    /// When the message was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Defines the possible types of messages
/// </summary>
public enum MessageType
{
    /// <summary>
    /// Message from the user
    /// </summary>
    User,

    /// <summary>
    /// Message from the system
    /// </summary>
    System
}