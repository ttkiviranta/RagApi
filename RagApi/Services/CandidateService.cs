using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDocumentService _documentService;

        public CandidateService(
            IUnitOfWork unitOfWork,
            IDocumentService documentService)
        {
            _unitOfWork = unitOfWork;
            _documentService = documentService;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Candidate>> GetAllAsync()
        {
            return await _unitOfWork.Candidates.GetAllAsync();
        }

        /// <inheritdoc/>
        public async Task<Candidate> GetByIdAsync(string id)
        {
            return await _unitOfWork.Candidates.GetByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<Candidate> CreateAsync(CandidateCreateDto dto, string? userId)
        {
            // userId validointi voidaan tehdä repositoryssa tai servicessä
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
                ResumeDocumentId = null,
                CoverLetterDocumentId = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = userId
            };

            try
            {
                await _unitOfWork.Candidates.AddAsync(candidate);
                await _unitOfWork.CommitAsync();
                return candidate;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while saving the candidate. See inner exception for details.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Candidate> UpdateAsync(string id, CandidateUpdateDto dto)
        {
            var candidate = await _unitOfWork.Candidates.GetByIdAsync(id);
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

            await _unitOfWork.Candidates.UpdateAsync(candidate);
            await _unitOfWork.CommitAsync();

            return candidate;
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string id)
        {
            var candidate = await _unitOfWork.Candidates.GetByIdAsync(id);
            if (candidate != null)
            {
                await _unitOfWork.Candidates.DeleteAsync(candidate);
                await _unitOfWork.CommitAsync();
            }
        }

        /// <inheritdoc/>
        public async Task<string> UploadResumeAsync(string candidateId, IFormFile file, string userId)
        {
            var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
            if (candidate == null)
                throw new KeyNotFoundException("Candidate not found");

            // Dokumentin käsittely käyttää dokumenttipalvelua
            var documentId = await _documentService.UploadAndProcessDocumentAsync(
                file,
                "Resume",
                candidateId,
                userId);

            // Repository-tason metodi päivittää viitteen
            await _unitOfWork.Candidates.UpdateResumeDocumentAsync(candidateId, documentId);
            return documentId;
        }

        /// <inheritdoc/>
        public async Task<string> UploadCoverLetterAsync(string candidateId, IFormFile file, string userId)
        {
            var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
            if (candidate == null)
                throw new KeyNotFoundException("Candidate not found");

            // Dokumentin käsittely käyttää dokumenttipalvelua
            var documentId = await _documentService.UploadAndProcessDocumentAsync(
                file,
                "CoverLetter",
                candidateId,
                userId);

            // Repository-tason metodi päivittää viitteen
            await _unitOfWork.Candidates.UpdateCoverLetterDocumentAsync(candidateId, documentId);
            return documentId;
        }
    }
}
