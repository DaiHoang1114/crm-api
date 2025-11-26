using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MyCRM.Client.Dto;

namespace MyCRM.Client.Services;

public class KeycloakService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<KeycloakService> logger)
{
    private async Task<string> GetAdminTokenAsync()
    {
        var tokenEndpoint = $"{configuration["Keycloak:AdminUrl"]}/realms/master/protocol/openid-connect/token";

        var formData = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "client_id", "admin-cli" },
            { "username", configuration["Keycloak:AdminUsername"]! },
            { "password", configuration["Keycloak:AdminPassword"]! }
        };

        var response = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(formData));
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            logger.LogError("Failed to get admin token: {Error}", error);
            throw new Exception($"Failed to get admin token: {error}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);

        return tokenResponse.GetProperty("access_token").GetString()!;
    }

    public async Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        try
        {
            var token = await GetAdminTokenAsync();
            var realm = configuration["Keycloak:Realm"];
            var url = $"{configuration["Keycloak:AdminUrl"]}/admin/realms/{realm}/users/{userId}";

            var updateData = new
            {
                firstName = request.FirstName,
                lastName = request.LastName,
                email = request.Email,
                attributes = new Dictionary<string, List<string>>()
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(updateData),
                Encoding.UTF8,
                "application/json");

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.PutAsync(url, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("User {UserId} updated successfully", userId);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            logger.LogError("Failed to update user {UserId}: {Error}", userId, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user {UserId}", userId);
            return false;
        }
    }

    public async Task<UserInfo?> GetUserAsync(string userId)
    {
        try
        {
            var token = await GetAdminTokenAsync();
            var realm = configuration["Keycloak:Realm"];
            var url = $"{configuration["Keycloak:AdminUrl"]}/admin/realms/{realm}/users/{userId}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<JsonElement>(content);

            return new UserInfo(
                Id: user.GetProperty("id").GetString()!,
                Username: user.GetProperty("username").GetString()!,
                Email: user.TryGetProperty("email", out var email) ? email.GetString() : null,
                FirstName: user.TryGetProperty("firstName", out var firstName) ? firstName.GetString() : null,
                LastName: user.TryGetProperty("lastName", out var lastName) ? lastName.GetString() : null,
                EmailVerified: user.TryGetProperty("emailVerified", out var verified) && verified.GetBoolean()
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user {UserId}", userId);
            return null;
        }
    }

    // public record UpdateUserRequest
    // {
    //     public string? FirstName { get; set; }
    //     public string? LastName { get; set; }
    //     public string? Email { get; set; }
    // }

    // public record UserInfo
    // {
    //     public string Id { get; init; } = string.Empty;
    //     public string Username { get; init; } = string.Empty;
    //     public string? Email { get; init; }
    //     public string? FirstName { get; init; }
    //     public string? LastName { get; init; }
    //     public bool EmailVerified { get; init; }
    // }
}