using Riok.Mapperly.Abstractions;
using RagApi.Models;

namespace RagApi.Mapping
{
    /// <summary>
    /// Compile-time generated mapper for Applications
    /// </summary>
    [Mapper]
    public sealed partial class ApplicationMapper
    {
        /// <summary>
        /// Maps Application entity to ApplicationResponseDto
        /// </summary>
        [MapperIgnoreSource(nameof(Application.Interviews))]
        [MapperIgnoreSource(nameof(Application.Candidate))]
        [MapperIgnoreSource(nameof(Application.JobPosting))]
        public ApplicationResponseDto MapToDto(Application application)
        {
            if (application == null) return null;

            var dto = ToResponseDto(application);
            // Mapperly ei osaa käsitellä navigaatio-ominaisuuksia,
            // joten käsittelemme ne erikseen
            return dto;
        }

        // Luo osittainen metodi, joka käsittelee vain perusominaisuudet
        private partial ApplicationResponseDto ToResponseDto(Application application);

        /// <summary>
        /// Maps ApplicationCreateDto to Application entity
        /// </summary>
        public Application MapToEntity(ApplicationCreateDto dto)
        {
            if (dto == null) return null;

            var entity = ToEntity(dto);
            // Aseta oletusarvot kentille, joita ei ole DTO:ssa
            entity.AppliedDate = DateTime.UtcNow;
            entity.Status = "New";
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.Id = Guid.NewGuid().ToString();
            return entity;
        }

        private partial Application ToEntity(ApplicationCreateDto dto);

        /// <summary>
        /// Updates Application entity properties from ApplicationStatusUpdateDto
        /// </summary>
        public void MapToEntity(Application entity, ApplicationStatusUpdateDto dto)
        {
            if (entity == null || dto == null) return;

            UpdateFromStatusDto(entity, dto);
            entity.UpdatedAt = DateTime.UtcNow;
        }

        private partial void UpdateFromStatusDto(Application entity, ApplicationStatusUpdateDto dto);
    }
}