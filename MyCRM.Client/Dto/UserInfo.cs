namespace MyCRM.Client.Dto;

public record UserInfo(
    string Id,
    string Username,
    string? Email,
    string? FirstName,
    string? LastName,
    bool EmailVerified
);