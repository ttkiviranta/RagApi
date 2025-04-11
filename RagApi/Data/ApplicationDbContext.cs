using Microsoft.EntityFrameworkCore;
using RagApi.Models;

namespace RagApi.Data
{
    /// <summary>
    /// Database context for the RAG API application.
    /// Defines the database structure and relationships using Entity Framework Core.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the ApplicationDbContext class.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the conversations in the database.
        /// </summary>
        public DbSet<Conversation> Conversations { get; set; }

        /// <summary>
        /// Gets or sets the messages in the database.
        /// </summary>
        public DbSet<Message> Messages { get; set; }

        /// <summary>
        /// Gets or sets the system prompts in the database.
        /// </summary>
        public DbSet<SystemPrompt> SystemPrompts { get; set; }

        /// <summary>
        /// Gets or sets the users in the database.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the user-system prompt associations in the database.
        /// </summary>
        public DbSet<UserSystemPrompt> UserSystemPrompts { get; set; }

        /// <summary>
        /// Configures the model that was discovered by convention from the entity types.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Conversation-Message relationship
            modelBuilder.Entity<Conversation>()
                .HasMany(c => c.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure User-Conversation relationship
            modelBuilder.Entity<Conversation>()
                .HasOne<User>()
                .WithMany(u => u.Conversations)
                .HasForeignKey("UserId");

            // Configure User-SystemPrompt many-to-many relationship
            modelBuilder.Entity<UserSystemPrompt>()
                .HasKey(usp => new { usp.UserId, usp.SystemPromptId });

            modelBuilder.Entity<UserSystemPrompt>()
                .HasOne(usp => usp.User)
                .WithMany(u => u.UserSystemPrompts)
                .HasForeignKey(usp => usp.UserId);

            modelBuilder.Entity<UserSystemPrompt>()
                .HasOne(usp => usp.SystemPrompt)
                .WithMany(sp => sp.UserSystemPrompts)
                .HasForeignKey(usp => usp.SystemPromptId);

            // Configure default system prompt constraint
            modelBuilder.Entity<SystemPrompt>()
                .HasIndex(sp => sp.IsDefault)
                .HasFilter("[IsDefault] = 1")
                .IsUnique();

            // Configure indexes
            modelBuilder.Entity<Conversation>()
                .HasIndex(c => c.UpdatedAt)
                .IsDescending();

            modelBuilder.Entity<Message>()
                .HasIndex(m => new { m.ConversationId, m.CreatedAt });

            modelBuilder.Entity<SystemPrompt>()
                .HasIndex(sp => sp.Name)
                .IsUnique();

            // Configure property constraints
            modelBuilder.Entity<Conversation>()
                .Property(c => c.Title)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<Message>()
                .Property(m => m.Content)
                .IsRequired();

            // Configure column types for JSON data
            modelBuilder.Entity<Message>()
                .Property(m => m.SearchResultsJson)
                .HasColumnType("nvarchar(max)");
        }
    }
}