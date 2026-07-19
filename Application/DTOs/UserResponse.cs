using Shortly.Domain.Entities;

namespace Shortly.Application.DTOs;

public class UserResponse
{
    public long Id { get; set; }
    public string Email { get; set; } = null!;

    public static UserResponse From(User user) => new()
    {
        Id = user.Id,
        Email = user.Email
    };
}
