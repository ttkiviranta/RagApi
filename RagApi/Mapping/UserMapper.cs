using Riok.Mapperly.Abstractions;
using RagApi.Models;
using RagApi.Models.Dto;

namespace RagApi.Mapping
{
    /// <summary>
    /// Compile-time generated mapper for User
    /// </summary>
    [Mapper]
    public sealed partial class UserMapper
    {
        /// <summary>
        /// Maps User entity to UserResponseDto
        /// </summary>
        [MapperIgnoreSource(nameof(User.UserSystemPrompts))]
        [MapperIgnoreSource(nameof(User.Conversations))]
        public UserResponseDto MapToDto(User user) => ToResponseDto(user);

        private partial UserResponseDto ToResponseDto(User user);
    }
}