using Riok.Mapperly.Abstractions;
using RagApi.Models;

namespace RagApi.Mapping
{
    /// <summary>
    /// Compile-time generated mapper for Candidates
    /// </summary>
    [Mapper]
    public sealed partial class CandidateMapper
    {
        /// <summary>
        /// Maps Candidate entity to CandidateResponseDto
        /// </summary>
        [MapperIgnoreSource(nameof(Candidate.Applications))]
        [MapperIgnoreSource(nameof(Candidate.User))]
        public CandidateResponseDto MapToDto(Candidate candidate)
        {
            if (candidate == null) return null;
            return ToResponseDto(candidate);
        }

        private partial CandidateResponseDto ToResponseDto(Candidate candidate);

        /// <summary>
        /// Maps CandidateCreateDto to Candidate entity
        /// </summary>
        public Candidate MapToEntity(CandidateCreateDto dto)
        {
            if (dto == null) return null;

            var entity = ToEntity(dto);
            entity.Id = Guid.NewGuid().ToString();
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            return entity;
        }

        private partial Candidate ToEntity(CandidateCreateDto dto);

        /// <summary>
        /// Updates Candidate entity properties from CandidateUpdateDto
        /// </summary>
        public void MapToEntity(Candidate entity, CandidateUpdateDto dto)
        {
            if (entity == null || dto == null) return;

            UpdateFromDto(entity, dto);
            entity.UpdatedAt = DateTime.UtcNow;
        }

        private partial void UpdateFromDto(Candidate entity, CandidateUpdateDto dto);
    }
}
