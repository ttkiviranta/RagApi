using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RagApi.Api.Models;
using RagApi.Interfaces;
using RagApi.Models;
using RagApi.Models.Dto;

namespace RagApi.Services
{
    /// <summary>
    /// Implementation of candidate service interface for managing candidate-related operations
    /// </summary>
    public class CandidateService : ICandidateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDocumentService _documentService;
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the CandidateService class
        /// </summary>
        /// <param name="unitOfWork">Unit of work for database operations</param>
        /// <param name="documentService">Service for document operations</param>
        /// <param name="userService">Service for user operations</param>
        public CandidateService(
            IUnitOfWork unitOfWork,
            IDocumentService documentService,
            IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _documentService = documentService;
            _userService = userService;
        }

        /// <summary>
        /// Gets all candidates from the database
        /// </summary>
        /// <returns>Collection of all candidates</returns>
        public async Task<IEnumerable<Candidate>> GetAllAsync()
        {
            return await _unitOfWork.Candidates.GetAllAsync();
        }

        /// <summary>
        /// Gets a candidate by their unique identifier
        /// </summary>
        /// <param name="id">The ID of the candidate to retrieve</param>
        /// <returns>The candidate if found, null otherwise</returns>
        public async Task<Candidate> GetByIdAsync(string id)
        {
            return await _unitOfWork.Candidates.GetByIdAsync(id);
        }

        /// <summary>
        /// Creates a new candidate in the system
        /// If the provided userId doesn't exist, creates a new user automatically
        /// </summary>
        /// <param name="dto">Data transfer object containing candidate information</param>
        /// <param name="userId">Optional user ID to associate with the candidate</param>
        /// <returns>The newly created candidate</returns>
        public async Task<Candidate> CreateAsync(CandidateCreateDto dto, string? userId)
        {
            // If userId is provided, verify that the user exists
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                // If user doesn't exist, create a new one
                if (user == null)
                {
                    try
                    {
                        // Create user request with data from the candidate
                        var createUserRequest = new CreateUserRequest
                        {
                            Username = $"{dto.FirstName} {dto.LastName}",
                            Email = dto.Email
                        };

                        // Use user service to create the user
                        user = await _userService.CreateUserAsync(createUserRequest);
                        userId = user.Id; // Update userId with newly created user ID
                    }
                    catch (Exception ex)
                    {
                        // If user creation fails, set userId to null but continue creating the candidate
                        userId = null;
                        // Log error here if needed
                    }
                }
            }

            // Create new candidate entity
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
                UserId = userId // Link to user if available
            };

            try
            {
                // Save the candidate to the database
                await _unitOfWork.Candidates.AddAsync(candidate);
                await _unitOfWork.CommitAsync();
                return candidate;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while saving the candidate. See inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Updates an existing candidate with new information
        /// </summary>
        /// <param name="id">ID of the candidate to update</param>
        /// <param name="dto">Data transfer object containing updated candidate information</param>
        /// <returns>The updated candidate if found, null otherwise</returns>
        public async Task<Candidate> UpdateAsync(string id, CandidateUpdateDto dto)
        {
            var candidate = await _unitOfWork.Candidates.GetByIdAsync(id);
            if (candidate == null)
                return null;

            // Update candidate properties
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

            // Save changes to the database
            await _unitOfWork.Candidates.UpdateAsync(candidate);
            await _unitOfWork.CommitAsync();

            return candidate;
        }

        /// <summary>
        /// Deletes a candidate from the system
        /// </summary>
        /// <param name="id">ID of the candidate to delete</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task DeleteAsync(string id)
        {
            var candidate = await _unitOfWork.Candidates.GetByIdAsync(id);
            if (candidate != null)
            {
                await _unitOfWork.Candidates.DeleteAsync(candidate);
                await _unitOfWork.CommitAsync();
            }
        }

        /// <summary>
        /// Uploads a resume document for a candidate
        /// </summary>
        /// <param name="candidateId">ID of the candidate</param>
        /// <param name="file">The resume file to upload</param>
        /// <param name="userId">ID of the user performing the upload</param>
        /// <returns>Document ID of the uploaded resume</returns>
        public async Task<string> UploadResumeAsync(string candidateId, IFormFile file, string userId)
        {
            var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
            if (candidate == null)
                throw new KeyNotFoundException("Candidate not found");

            // Use document service to process and store the document
            var documentId = await _documentService.UploadAndProcessDocumentAsync(
                file,
                "Resume",
                candidateId,
                userId);

            // Update candidate's resume reference
            await _unitOfWork.Candidates.UpdateResumeDocumentAsync(candidateId, documentId);
            return documentId;
        }

        /// <summary>
        /// Uploads a cover letter document for a candidate
        /// </summary>
        /// <param name="candidateId">ID of the candidate</param>
        /// <param name="file">The cover letter file to upload</param>
        /// <param name="userId">ID of the user performing the upload</param>
        /// <returns>Document ID of the uploaded cover letter</returns>
        public async Task<string> UploadCoverLetterAsync(string candidateId, IFormFile file, string userId)
        {
            var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
            if (candidate == null)
                throw new KeyNotFoundException("Candidate not found");

            // Use document service to process and store the document
            var documentId = await _documentService.UploadAndProcessDocumentAsync(
                file,
                "CoverLetter",
                candidateId,
                userId);

            // Update candidate's cover letter reference
            await _unitOfWork.Candidates.UpdateCoverLetterDocumentAsync(candidateId, documentId);
            return documentId;
        }
    }
}
