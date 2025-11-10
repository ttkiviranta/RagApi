using System.Collections.ObjectModel;

namespace RagMaui.Tests.TestInfrastructure
{
    /// <summary>
    /// Simple models for testing purposes
    /// </summary>
    public class TestModels
    {
        public class RagResponse
        {
         public string Answer { get; set; } = string.Empty;
      public List<SearchResult> SourceResults { get; set; } = new List<SearchResult>();
        }

  public class SearchResult
  {
            public string Title { get; set; } = string.Empty;
      public string Content { get; set; } = string.Empty;
        }

        public class Conversation
        {
            public string Id { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
  public List<Message> Messages { get; set; } = new List<Message>();
        }

        public class Message
        {
     public string Content { get; set; } = string.Empty;
     public bool IsFromUser { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class JobPosting
        {
       public string Id { get; set; } = string.Empty;
      public string Title { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
   }

        public class Candidate
        {
        public string Id { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
          public string LastName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        public class MatchResult
        {
        public string JobId { get; set; } = string.Empty;
      public decimal MatchScore { get; set; }
        public string Analysis { get; set; } = string.Empty;
        }

    public class SystemPrompt
        {
    public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
         public string Description { get; set; } = string.Empty;
        }

        public class JobApplication
        {
     public string Id { get; set; } = string.Empty;
      public string CandidateId { get; set; } = string.Empty;
     public string JobPostingId { get; set; } = string.Empty;
         public string Status { get; set; } = string.Empty;
        }

 public class Interview
     {
    public string Id { get; set; } = string.Empty;
            public string ApplicationId { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
        }
    }

    /// <summary>
    /// Interface for testing API service
    /// </summary>
    public interface ITestRagApiService
    {
   Task<TestModels.RagResponse> QueryAsync(string query);
        Task<TestModels.RagResponse> QueryInConversationAsync(string conversationId, string query);
        Task<TestModels.Conversation> CreateConversationAsync(string title);
        Task<TestModels.Conversation> GetConversationAsync(string conversationId);
   Task<IEnumerable<TestModels.Conversation>> GetConversationsAsync();
        Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string documentType, string entityId);
        Task<IEnumerable<TestModels.JobPosting>> GetJobPostingsAsync();
        Task<IEnumerable<TestModels.Candidate>> GetCandidatesAsync();
        Task<IEnumerable<TestModels.MatchResult>> MatchCandidateWithJobsAsync(string candidateId, int maxResults);
        Task<IEnumerable<TestModels.SystemPrompt>> GetSystemPromptsAsync();
      Task<IEnumerable<TestModels.JobApplication>> GetApplicationsAsync();
        Task<IEnumerable<TestModels.Interview>> GetInterviewsAsync();
    }

    /// <summary>
    /// Mock implementation of API service for testing
    /// </summary>
    public class MockRagApiService : ITestRagApiService
    {
        public Task<TestModels.RagResponse> QueryAsync(string query)
        {
         return Task.FromResult(new TestModels.RagResponse
       {
        Answer = $"Mock response for: {query}",
      SourceResults = new List<TestModels.SearchResult>
      {
    new TestModels.SearchResult { Title = "Test Document", Content = "Test content" }
        }
            });
        }

        public Task<TestModels.RagResponse> QueryInConversationAsync(string conversationId, string query)
      {
        return QueryAsync(query);
   }

        public Task<TestModels.Conversation> CreateConversationAsync(string title)
        {
     return Task.FromResult(new TestModels.Conversation
            {
Id = Guid.NewGuid().ToString(),
                Title = title,
                CreatedAt = DateTime.UtcNow,
              Messages = new List<TestModels.Message>()
  });
 }

        public Task<TestModels.Conversation> GetConversationAsync(string conversationId)
   {
        return Task.FromResult(new TestModels.Conversation
   {
      Id = conversationId,
   Title = "Test Conversation",
 CreatedAt = DateTime.UtcNow,
     Messages = new List<TestModels.Message>()
});
        }

        public Task<IEnumerable<TestModels.Conversation>> GetConversationsAsync()
        {
     var conversations = new List<TestModels.Conversation>
      {
    new TestModels.Conversation { Id = "1", Title = "Test Conversation 1", CreatedAt = DateTime.UtcNow },
    new TestModels.Conversation { Id = "2", Title = "Test Conversation 2", CreatedAt = DateTime.UtcNow }
         };
       return Task.FromResult<IEnumerable<TestModels.Conversation>>(conversations);
        }

        public Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string documentType, string entityId)
        {
  return Task.FromResult(Guid.NewGuid().ToString());
        }

public Task<IEnumerable<TestModels.JobPosting>> GetJobPostingsAsync()
    {
      var jobPostings = new List<TestModels.JobPosting>
  {
      new TestModels.JobPosting { Id = "1", Title = "Software Developer", Company = "Test Company" },
            new TestModels.JobPosting { Id = "2", Title = "Data Analyst", Company = "Another Company" }
};
       return Task.FromResult<IEnumerable<TestModels.JobPosting>>(jobPostings);
        }

 public Task<IEnumerable<TestModels.Candidate>> GetCandidatesAsync()
 {
            var candidates = new List<TestModels.Candidate>
            {
     new TestModels.Candidate { Id = "1", FirstName = "John", LastName = "Doe", Email = "john@example.com" },
                new TestModels.Candidate { Id = "2", FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
            };
         return Task.FromResult<IEnumerable<TestModels.Candidate>>(candidates);
        }

        public Task<IEnumerable<TestModels.MatchResult>> MatchCandidateWithJobsAsync(string candidateId, int maxResults)
      {
      var matches = new List<TestModels.MatchResult>
            {
    new TestModels.MatchResult { JobId = "1", MatchScore = 85.5m, Analysis = "Good match" },
         new TestModels.MatchResult { JobId = "2", MatchScore = 72.3m, Analysis = "Moderate match" }
       };
        return Task.FromResult<IEnumerable<TestModels.MatchResult>>(matches);
        }

   public Task<IEnumerable<TestModels.SystemPrompt>> GetSystemPromptsAsync()
        {
            var prompts = new List<TestModels.SystemPrompt>
        {
            new TestModels.SystemPrompt { Id = "1", Name = "Test Prompt", Description = "Test description" }
            };
            return Task.FromResult<IEnumerable<TestModels.SystemPrompt>>(prompts);
        }

 public Task<IEnumerable<TestModels.JobApplication>> GetApplicationsAsync()
        {
   var applications = new List<TestModels.JobApplication>
            {
  new TestModels.JobApplication { Id = "1", CandidateId = "1", JobPostingId = "1", Status = "New" }
       };
        return Task.FromResult<IEnumerable<TestModels.JobApplication>>(applications);
     }

        public Task<IEnumerable<TestModels.Interview>> GetInterviewsAsync()
  {
            var interviews = new List<TestModels.Interview>
         {
                new TestModels.Interview { Id = "1", ApplicationId = "1", ScheduledDate = DateTime.UtcNow.AddDays(7) }
};
            return Task.FromResult<IEnumerable<TestModels.Interview>>(interviews);
        }
    }
}