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
        /// Gets or sets the candidates in the database.
        /// </summary>
        public DbSet<Candidate> Candidates { get; set; }

        /// <summary>
        /// Gets or sets the job postings in the database.
        /// </summary>
        public DbSet<JobPosting> JobPostings { get; set; }

        /// <summary>
        /// Gets or sets the job applications in the database.
        /// </summary>
        public DbSet<Application> Applications { get; set; }

        /// <summary>
        /// Gets or sets the interviews in the database.
        /// </summary>
        public DbSet<Interview> Interviews { get; set; }

        /// <summary>
        /// Gets or sets the documents in the database.
        /// </summary>
        public DbSet<Document> Documents { get; set; }

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

            // Configure decimal precision
            modelBuilder.Entity<Application>()
                .Property(a => a.MatchScore)
                .HasColumnType("decimal(5, 2)"); // 5 digits total, 2 after decimal point

            modelBuilder.Entity<JobPosting>()
                .Property(jp => jp.SalaryMin)
                .HasColumnType("decimal(12, 2)"); // 12 digits total, 2 after decimal point

            modelBuilder.Entity<JobPosting>()
                .Property(jp => jp.SalaryMax)
                .HasColumnType("decimal(12, 2)"); // 12 digits total, 2 after decimal point

            // Configure Candidate relationships
            modelBuilder.Entity<Candidate>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure JobPosting relationships
            modelBuilder.Entity<JobPosting>()
                .HasOne(jp => jp.CreatedByUser)
                .WithMany()
                .HasForeignKey(jp => jp.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Application relationships
            modelBuilder.Entity<Application>()
                .HasOne(a => a.Candidate)
                .WithMany(c => c.Applications)
                .HasForeignKey(a => a.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Application>()
                .HasOne(a => a.JobPosting)
                .WithMany(jp => jp.Applications)
                .HasForeignKey(a => a.JobPostingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Interview relationships
            modelBuilder.Entity<Interview>()
                .HasOne(i => i.Application)
                .WithMany(a => a.Interviews)
                .HasForeignKey(i => i.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Interview>()
                .HasOne(i => i.Interviewer)
                .WithMany()
                .HasForeignKey(i => i.InterviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Document relationships
            modelBuilder.Entity<Document>()
                .HasOne(d => d.UploadedByUser)
                .WithMany()
                .HasForeignKey(d => d.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure property constraints for recruitment entities
            modelBuilder.Entity<Candidate>()
                .Property(c => c.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Candidate>()
                .Property(c => c.LastName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Candidate>()
                .Property(c => c.Email)
                .HasMaxLength(255)
                .IsRequired();

            modelBuilder.Entity<JobPosting>()
                .Property(jp => jp.Title)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<JobPosting>()
                .Property(jp => jp.Description)
                .IsRequired();

            modelBuilder.Entity<Application>()
                .Property(a => a.Status)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Interview>()
                .Property(i => i.InterviewType)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Interview>()
                .Property(i => i.Status)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Document>()
                .Property(d => d.FileName)
                .HasMaxLength(255)
                .IsRequired();

            modelBuilder.Entity<Document>()
                .Property(d => d.DocumentType)
                .HasMaxLength(50)
                .IsRequired();
        }
    }
}