using System;

namespace RagApi.Models.Dto
{
    public record UserResponseDto(
        string Id,
        string Username,
        string Email,
        bool IsAdmin,
        DateTime CreatedAt,
        DateTime? LastLogin);
}
