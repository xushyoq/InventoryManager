using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace InventoryManager.Services;

public class SalesforceService : ISalesforceService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SalesforceService> _logger;

    public SalesforceService(HttpClient httpClient, IConfiguration configuration, ILogger<SalesforceService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> CreateAccountAndContactAsync(
        string name,
        string email,
        string phone,
        string company,
        string jobTitle,
        CancellationToken ct = default)
    {
        var (accessToken, instanceUrl) = await GetAccessTokenAsync(ct);
        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(instanceUrl))
            return false;

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Create Account
        var accountBody = JsonSerializer.Serialize(new { Name = company });
        var accountResponse = await _httpClient.PostAsync(
            $"{instanceUrl}/services/data/v59.0/sobjects/Account",
            new StringContent(accountBody, Encoding.UTF8, "application/json"), ct);

        if (!accountResponse.IsSuccessStatusCode)
        {
            var err = await accountResponse.Content.ReadAsStringAsync(ct);
            _logger.LogError("Salesforce Account creation failed: {Status} {Body}", accountResponse.StatusCode, err);
            return false;
        }

        var accountJson = await accountResponse.Content.ReadAsStringAsync(ct);
        using var accountDoc = JsonDocument.Parse(accountJson);
        var accountId = accountDoc.RootElement.GetProperty("id").GetString();

        // Split name into first/last
        var nameParts = name.Trim().Split(' ', 2);
        var firstName = nameParts.Length > 1 ? nameParts[0] : "";
        var lastName = nameParts.Length > 1 ? nameParts[1] : nameParts[0];

        // Create Contact
        var contactBody = JsonSerializer.Serialize(new
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            Title = jobTitle,
            AccountId = accountId
        });

        var contactResponse = await _httpClient.PostAsync(
            $"{instanceUrl}/services/data/v59.0/sobjects/Contact",
            new StringContent(contactBody, Encoding.UTF8, "application/json"), ct);

        if (!contactResponse.IsSuccessStatusCode)
        {
            var err = await contactResponse.Content.ReadAsStringAsync(ct);
            _logger.LogError("Salesforce Contact creation failed: {Status} {Body}", contactResponse.StatusCode, err);
            return false;
        }

        _logger.LogInformation("Salesforce Account and Contact created for {Email}", email);
        return true;
    }

    private async Task<(string? accessToken, string? instanceUrl)> GetAccessTokenAsync(CancellationToken ct)
    {
        var authUrl = _configuration["Salesforce:AuthUrl"];
        var clientId = _configuration["Salesforce:ClientId"];
        var clientSecret = _configuration["Salesforce:ClientSecret"];
        var username = _configuration["Salesforce:Username"];
        var password = _configuration["Salesforce:Password"];

        if (string.IsNullOrWhiteSpace(authUrl) || string.IsNullOrWhiteSpace(clientId) ||
            string.IsNullOrWhiteSpace(clientSecret) || string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Salesforce integration is not configured.");
            return (null, null);
        }

        var formData = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["username"] = username,
            ["password"] = password
        });

        var response = await _httpClient.PostAsync(authUrl, formData, ct);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Salesforce OAuth failed: {Status} {Body}", response.StatusCode, err);
            return (null, null);
        }

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        var token = doc.RootElement.GetProperty("access_token").GetString();
        var url = doc.RootElement.GetProperty("instance_url").GetString();
        return (token, url);
    }
}
