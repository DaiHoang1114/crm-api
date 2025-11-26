namespace MyCRM.Client.Dto;

public record UpdateUserRequest(
    string? FirstName,
    string? LastName,
    string? Email
);