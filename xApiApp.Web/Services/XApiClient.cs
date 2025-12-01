using System.Net.Http.Json;
using System.Text.Json;
using xApiApp.Web.Models;

namespace xApiApp.Web.Services;

public class XApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<XApiClient> _logger;

    public XApiClient(HttpClient httpClient, ILogger<XApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AboutResponse?> GetAboutAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AboutResponse>("/xapi/about");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching about information");
            return null;
        }
    }

    public async Task<List<string>> PostStatementAsync(Statement statement)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Experience-API-Version");
            _httpClient.DefaultRequestHeaders.Add("X-Experience-API-Version", "1.0.3");
            
            var response = await _httpClient.PostAsJsonAsync("/xapi/statements", statement);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<string>>() ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting statement");
            throw;
        }
    }

    public async Task<StatementResult?> GetStatementsAsync(int limit = 50, bool ascending = false)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Experience-API-Version");
            _httpClient.DefaultRequestHeaders.Add("X-Experience-API-Version", "1.0.3");
            
            var url = $"/xapi/statements?limit={limit}&ascending={ascending}";
            return await _httpClient.GetFromJsonAsync<StatementResult>(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching statements");
            return null;
        }
    }

    public async Task<Statement?> GetStatementAsync(string statementId)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Experience-API-Version");
            _httpClient.DefaultRequestHeaders.Add("X-Experience-API-Version", "1.0.3");
            
            return await _httpClient.GetFromJsonAsync<Statement>($"/xapi/statements?statementId={statementId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching statement");
            return null;
        }
    }
}

