using FluentAssertions;
using RagMaui.Tests.TestInfrastructure;
using Xunit;

namespace RagMaui.Tests.Services
{
 /// <summary>
/// Unit tests for API service functionality
    /// </summary>
  public class RagApiServiceTests
    {
     [Fact]
        public async Task MockRagApiService_QueryAsync_ShouldReturnValidResponse()
        {
    // Arrange
            var service = new MockRagApiService();
       var query = "What is artificial intelligence?";

       // Act
    var result = await service.QueryAsync(query);

        // Assert
  result.Should().NotBeNull();
       result.Answer.Should().Contain(query);
      result.SourceResults.Should().NotBeEmpty();
result.SourceResults.First().Title.Should().Be("Test Document");
  }

[Fact]
public async Task MockRagApiService_CreateConversationAsync_ShouldReturnValidConversation()
{
    // Arrange
     var service = new MockRagApiService();
  var title = "Test Conversation Title";

  // Act
         var result = await service.CreateConversationAsync(title);

// Assert
          result.Should().NotBeNull();
 result.Title.Should().Be(title);
  result.Id.Should().NotBeEmpty();
   result.Messages.Should().NotBeNull();
         result.Messages.Should().BeEmpty();
      }

    [Fact]
        public async Task MockRagApiService_GetCandidatesAsync_ShouldReturnCandidateList()
 {
    // Arrange
       var service = new MockRagApiService();

     // Act
      var result = await service.GetCandidatesAsync();

      // Assert
 result.Should().NotBeNull();
       result.Should().HaveCount(2);
   
  var firstCandidate = result.First();
      firstCandidate.FirstName.Should().Be("John");
            firstCandidate.LastName.Should().Be("Doe");
     firstCandidate.Email.Should().Be("john@example.com");
}

        [Fact]
      public async Task MockRagApiService_GetJobPostingsAsync_ShouldReturnJobPostingList()
        {
     // Arrange
   var service = new MockRagApiService();

       // Act
      var result = await service.GetJobPostingsAsync();

   // Assert
     result.Should().NotBeNull();
  result.Should().HaveCount(2);
        
         var firstJobPosting = result.First();
      firstJobPosting.Title.Should().Be("Software Developer");
 firstJobPosting.Company.Should().Be("Test Company");
        }

 [Fact]
  public async Task MockRagApiService_MatchCandidateWithJobsAsync_ShouldReturnMatches()
     {
            // Arrange
    var service = new MockRagApiService();
       var candidateId = "test-candidate-id";
    var maxResults = 5;

            // Act
       var result = await service.MatchCandidateWithJobsAsync(candidateId, maxResults);

      // Assert
result.Should().NotBeNull();
       result.Should().HaveCount(2);
        
       var firstMatch = result.First();
 firstMatch.MatchScore.Should().Be(85.5m);
            firstMatch.Analysis.Should().Be("Good match");
    }

    [Fact]
  public async Task MockRagApiService_GetConversationsAsync_ShouldReturnConversationList()
  {
      // Arrange
      var service = new MockRagApiService();

   // Act
var result = await service.GetConversationsAsync();

        // Assert
  result.Should().NotBeNull();
 result.Should().HaveCount(2);
   
         var firstConversation = result.First();
       firstConversation.Title.Should().Be("Test Conversation 1");
      firstConversation.Id.Should().Be("1");
    }

        [Fact]
    public async Task MockRagApiService_UploadDocumentAsync_ShouldReturnDocumentId()
     {
 // Arrange
     var service = new MockRagApiService();
 var fileStream = new MemoryStream();
   var fileName = "test.pdf";
        var documentType = "Resume";
    var entityId = "candidate-1";

     // Act
         var result = await service.UploadDocumentAsync(fileStream, fileName, documentType, entityId);

     // Assert
         result.Should().NotBeEmpty();
    Guid.TryParse(result, out _).Should().BeTrue();
  }
    }
}