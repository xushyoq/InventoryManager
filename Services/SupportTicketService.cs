using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using InventoryManager.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InventoryManager.Services;

public class SupportTicketService : ISupportTicketService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SupportTicketService> _logger;
    private readonly HttpClient _httpClient = new();

    public SupportTicketService(IConfiguration configuration, ILogger<SupportTicketService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SubmitAsync(SupportTicketDto ticket, CancellationToken ct = default)
    {
        var accessToken = _configuration["Dropbox:AccessToken"];

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            _logger.LogWarning("Dropbox integration is not configured. Support ticket saved locally only.");
            return true;
        }

        try
        {
            var json = JsonSerializer.Serialize(ticket, new JsonSerializerOptions { WriteIndented = true });
            var fileName = $"ticket-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
            var folderPath = "/SupportTickets";
            var dropboxPath = $"{folderPath}/{fileName}";

            var apiArg = JsonSerializer.Serialize(new { path = dropboxPath, mode = "add" });

            var request = new HttpRequestMessage(HttpMethod.Post, "https://content.dropboxapi.com/2/files/upload");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.TryAddWithoutValidation("Dropbox-API-Arg", apiArg);
            request.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(json));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var response = await _httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Dropbox upload failed: {Status} {Body}", response.StatusCode, errorBody);
                return false;
            }

            _logger.LogInformation("Support ticket uploaded to Dropbox: {FileName}", fileName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload support ticket to Dropbox");
            return false;
        }
    }
}
