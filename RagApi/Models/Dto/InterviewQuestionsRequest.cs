// DTO for interview question generation requests
namespace RagApi.Models.Dto
{
    public class InterviewQuestionsRequest
    {
        public string JobPosting { get; set; }
        public string CandidateId { get; set; }
    }
}
