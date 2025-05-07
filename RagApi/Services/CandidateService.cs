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
        public async Task<Candidate> CreateAsync(CandidateCreateDto dto, string? userId)
        {
            // Tarkista onko käyttäjä olemassa, jos userId on annettu
            if (!string.IsNullOrEmpty(userId))
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    // Käyttäjää ei löydy, asetetaan userId nulliksi
                    userId = null;
                }
            }

            var candidate = new Candidate
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber ?? string.Empty,
                LinkedInProfile = dto.LinkedInProfile ?? string.Empty,
                CurrentPosition = dto.CurrentPosition ?? string.Empty,
                CurrentCompany = dto.CurrentCompany ?? string.Empty,
                Skills = dto.Skills ?? string.Empty,
                Location = dto.Location ?? string.Empty,
                ResumeDocumentId = null,  // Käytä null arvoa tyhjän merkkijonon sijaan
                CoverLetterDocumentId = null, // Käytä null arvoa tyhjän merkkijonon sijaan
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = userId
            };

            _context.Candidates.Add(candidate);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("An error occurred while saving the candidate. See inner exception for details.", ex);
            }

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
            // Huom! UserId jätetään ennalleen

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

            // userId on vain dokumentin metatietoja varten, ei rajoita käyttöoikeuksia
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

            // userId on vain dokumentin metatietoja varten, ei rajoita käyttöoikeuksia
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
