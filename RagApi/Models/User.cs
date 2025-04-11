// Models/User.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RagApi.Models
{
    /// <summary>
    /// Represents a user in the system
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier for the user (Azure Entra ID object identifier)
        /// </summary>
        [Key]
        public string Id { get; set; }

        /// <summary>
        /// Username for identification
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        /// <summary>
        /// Email address of the user
        /// </summary>
        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Indicates if the user has admin privileges
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// When the user was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the user last logged in
        /// </summary>
        public DateTime LastLogin { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the user role was last synced from Azure
        /// </summary>
        public DateTime LastRoleSync { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// System prompts associated with this user
        /// </summary>
        public List<UserSystemPrompt> UserSystemPrompts { get; set; } = new List<UserSystemPrompt>();

        /// <summary>
        /// Conversations initiated by this user
        /// </summary>
        public List<Conversation> Conversations { get; set; } = new List<Conversation>();
    }
}
