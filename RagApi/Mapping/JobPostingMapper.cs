using Riok.Mapperly.Abstractions;
using RagApi.Models;

namespace RagApi.Mapping
{
    /// <summary>
    /// Compile-time generated mapper for JobPostings
    /// </summary>
    [Mapper]
    public sealed partial class JobPostingMapper
    {
        /// <summary>
        /// Maps JobPosting entity to JobPostingResponseDto
        /// </summary>
        [MapperIgnoreSource(nameof(JobPosting.Applications))]
        [MapperIgnoreSource(nameof(JobPosting.CreatedByUser))]
        public JobPostingResponseDto MapToDto(JobPosting jobPosting)
        {
            if (jobPosting == null) return null;
            return ToResponseDto(jobPosting);
        }

        private partial JobPostingResponseDto ToResponseDto(JobPosting jobPosting);

        /// <summary>
        /// Maps JobPostingCreateDto to JobPosting entity
        /// </summary>
        public JobPosting MapToEntity(JobPostingCreateDto dto)
        {
            if (dto == null) return null;

            var entity = ToEntity(dto);
            entity.Id = Guid.NewGuid().ToString();
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            return entity;
        }

        private partial JobPosting ToEntity(JobPostingCreateDto dto);

        /// <summary>
        /// Updates JobPosting entity properties from JobPostingUpdateDto
        /// </summary>
        public void MapToEntity(JobPosting entity, JobPostingUpdateDto dto)
        {
            if (entity == null || dto == null) return;

            UpdateFromDto(entity, dto);
            entity.UpdatedAt = DateTime.UtcNow;
        }

        private partial void UpdateFromDto(JobPosting entity, JobPostingUpdateDto dto);
    }
}
