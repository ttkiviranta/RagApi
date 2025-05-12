// RagApi/Interfaces/Repositories/ICandidateRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using RagApi.Models;

namespace RagApi.Interfaces.Repositories
{
    public interface ICandidateRepository : IRepository<Candidate>
    {
        Task<Candidate> GetByEmailAsync(string email);
        Task UpdateResumeDocumentAsync(string candidateId, string documentId);
        Task UpdateCoverLetterDocumentAsync(string candidateId, string documentId);
        Task<IEnumerable<Document>> GetCandidateDocumentsAsync(string candidateId);
    }
}
