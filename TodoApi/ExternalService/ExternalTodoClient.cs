
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace TodoApi.ExternalService;

public record ExternalTodoDto(int UserId, int Id, string Title, bool Completed);

public class ExternalTodoClient(HttpClient httpClient, IOptions<ExternalTodoClient.ExternalTodoApiOptions> options, ILogger<ExternalTodoClient> logger) : IExternalTodoClient
{
    public class ExternalTodoApiOptions
    {
        public string TodosUrl { get; set; } = string.Empty;
    }
    private readonly HttpClient _httpClient = httpClient;

    private readonly ExternalTodoApiOptions _options = options.Value;

    private readonly ILogger<ExternalTodoClient> _logger = logger;

    public async Task<IEnumerable<ExternalTodoDto>> GetTodosAsync(CancellationToken token)
    {
        try {
            var response = await _httpClient.GetAsync(_options.TodosUrl, token);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<ExternalTodoDto>>(token)
                   ?? [];
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Fetching external todos was canceled.");
            return [];
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP error fetching external todos");
            return [];
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON deserialization error fetching external todos");
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching external todos");
            return [];
        }
    }
}
