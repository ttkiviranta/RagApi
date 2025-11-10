// Models/SystemPrompt.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RagApi.Models;

/// <summary>
/// Represents a system prompt template that defines how ChatGPT should behave
/// </summary>
public class SystemPrompt
{
    /// <summary>
    /// Unique identifier for the system prompt
    /// </summary>
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Name of the system prompt for easy identification
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of what this system prompt does
    /// </summary>
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The actual prompt text that will be sent to ChatGPT
    /// </summary>
    [Required]
    public string PromptText { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the default system prompt
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// When the system prompt was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the system prompt was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Users associated with this system prompt
    /// </summary>
    public List<UserSystemPrompt> UserSystemPrompts { get; set; } = new List<UserSystemPrompt>();
}

/// <summary>
/// Join table for many-to-many relationship between users and system prompts
/// </summary>
public class UserSystemPrompt
{
    /// <summary>
    /// ID of the user
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the user
    /// </summary>
    [ForeignKey("UserId")]
    public User? User { get; set; }

    /// <summary>
    /// ID of the system prompt
    /// </summary>
    public string SystemPromptId { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the system prompt
    /// </summary>
    [ForeignKey("SystemPromptId")]
    public SystemPrompt? SystemPrompt { get; set; }

    /// <summary>
    /// Is this the user's default system prompt
    /// </summary>
    public bool IsDefault { get; set; }
}