using Riok.Mapperly.Abstractions;
using RagApi.Models;

namespace RagApi.Mapping
{
    /// <summary>
    /// Compile-time generated mapper for Interviews
    /// </summary>
    [Mapper]
    public sealed partial class InterviewMapper
    {
        /// <summary>
        /// Maps Interview entity to InterviewResponseDto
        /// </summary>
        [MapperIgnoreSource(nameof(Interview.Application))]
        [MapperIgnoreSource(nameof(Interview.Interviewer))]
        public InterviewResponseDto MapToDto(Interview interview)
        {
            if (interview == null) return null;
            return ToResponseDto(interview);
        }

        private partial InterviewResponseDto ToResponseDto(Interview interview);

        /// <summary>
        /// Maps InterviewCreateDto to Interview entity
        /// </summary>
        public Interview MapToEntity(InterviewCreateDto dto)
        {
            if (dto == null) return null;

            var entity = ToEntity(dto);
            entity.Id = Guid.NewGuid().ToString();
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            return entity;
        }

        private partial Interview ToEntity(InterviewCreateDto dto);

        /// <summary>
        /// Updates Interview entity properties from InterviewUpdateDto
        /// </summary>
        public void MapToEntity(Interview entity, InterviewUpdateDto dto)
        {
            if (entity == null || dto == null) return;

            UpdateFromDto(entity, dto);
            entity.UpdatedAt = DateTime.UtcNow;
        }

        private partial void UpdateFromDto(Interview entity, InterviewUpdateDto dto);
    }
}