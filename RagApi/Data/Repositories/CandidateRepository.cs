// RagApi/Data/Repositories/CandidateRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RagApi.Interfaces.Repositories;
using RagApi.Models;

namespace RagApi.Data.Repositories
{
    public class CandidateRepository : Repository<Candidate>, ICandidateRepository
    {
        public CandidateRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Candidate> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task UpdateResumeDocumentAsync(string candidateId, string documentId)
        {
            var candidate = await GetByIdAsync(candidateId);
            if (candidate != null)
            {
                candidate.ResumeDocumentId = documentId;
                candidate.UpdatedAt = DateTime.UtcNow;
                await SaveChangesAsync();
            }
        }

        public async Task UpdateCoverLetterDocumentAsync(string candidateId, string documentId)
        {
            var candidate = await GetByIdAsync(candidateId);
            if (candidate != null)
            {
                candidate.CoverLetterDocumentId = documentId;
                candidate.UpdatedAt = DateTime.UtcNow;
                await SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Document>> GetCandidateDocumentsAsync(string candidateId)
        {
            return await _context.Documents
                .Where(d => d.EntityId == candidateId)
                .ToListAsync();
        }
    }
}

