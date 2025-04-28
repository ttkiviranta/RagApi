using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RagApi.Data;
using RagApi.Interfaces;
using RagApi.Models;
using RagApi.Models.Dto;

namespace RagApi.Services
{
    /// <summary>
    /// Implementation of candidate service interface
    /// </summary>
    public class CandidateService : ICandidateService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDocumentService _documentService;

        public CandidateService(
            ApplicationDbContext context,
            IDocumentService documentService)
        {
            _context = context;
            _documentService = documentService;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Candidate>> GetAllAsync()
        {
            return await _context.Candidates.ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Candidate> GetByIdAsync(string id)
        {
            return await _context.Candidates.FindAsync(id);
        }

        /// <inheritdoc/>
        public async Task<Candidate> CreateAsync(CandidateCreateDto dto, string userId)
        {
            var candidate = new Candidate
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                LinkedInProfile = dto.LinkedInProfile,
                CurrentPosition = dto.CurrentPosition,
                CurrentCompany = dto.CurrentCompany,
                Skills = dto.Skills,
                Location = dto.Location,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = userId
            };

            _context.Candidates.Add(candidate);
            await _context.SaveChangesAsync();

            return candidate;
        }

        /// <inheritdoc/>
        public async Task<Candidate> UpdateAsync(string id, CandidateUpdateDto dto)
        {
            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate == null)
                return null;

            candidate.FirstName = dto.FirstName;
            candidate.LastName = dto.LastName;
            candidate.Email = dto.Email;
            candidate.PhoneNumber = dto.PhoneNumber;
            candidate.LinkedInProfile = dto.LinkedInProfile;
            candidate.CurrentPosition = dto.CurrentPosition;
            candidate.CurrentCompany = dto.CurrentCompany;
            candidate.Skills = dto.Skills;
            candidate.Location = dto.Location;
            candidate.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return candidate;
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string id)
        {
            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate != null)
            {
                _context.Candidates.Remove(candidate);
                await _context.SaveChangesAsync();
            }
        }

        /// <inheritdoc/>
        public async Task<string> UploadResumeAsync(string candidateId, IFormFile file, string userId)
        {
            var candidate = await _context.Candidates.FindAsync(candidateId);
            if (candidate == null)
                throw new KeyNotFoundException("Candidate not found");

            // Upload and process document
            var documentId = await _documentService.UploadAndProcessDocumentAsync(
                file,
                "Resume",
                candidateId,
                userId);

            // Update candidate's resume reference
            candidate.ResumeDocumentId = documentId;
            candidate.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return documentId;
        }

        /// <inheritdoc/>
        public async Task<string> UploadCoverLetterAsync(string candidateId, IFormFile file, string userId)
        {
            var candidate = await _context.Candidates.FindAsync(candidateId);
            if (candidate == null)
                throw new KeyNotFoundException("Candidate not found");

            // Upload and process document
            var documentId = await _documentService.UploadAndProcessDocumentAsync(
                file,
                "CoverLetter",
                candidateId,
                userId);

            // Update candidate's cover letter reference
            candidate.CoverLetterDocumentId = documentId;
            candidate.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return documentId;
        }
    }
}
