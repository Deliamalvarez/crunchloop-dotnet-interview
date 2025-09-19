
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TodoApi.ExternalService;

public record ExternalTodoListDto([property: JsonPropertyName("id")] string Id, [property: JsonPropertyName("source_id")] string SourceId, [property: JsonPropertyName("name")] string Name, [property: JsonPropertyName("created_at")] DateTime CreatedAt, [property: JsonPropertyName("updated_at")] DateTime UpdatedAt, [property: JsonPropertyName("item")] IEnumerable<ExternalTodoItemDto> Items);

public record ExternalTodoItemDto([property: JsonPropertyName("id")] string Id, [property: JsonPropertyName("source_id")] string SourceId, [property: JsonPropertyName("description")] string Description, [property: JsonPropertyName("completed")] bool Completed, [property: JsonPropertyName("created_at")] DateTime CreatedAt, [property: JsonPropertyName("updated_at")] DateTime UpdatedAt);

public class ExternalTodoClient(HttpClient httpClient, IOptions<ExternalTodoClient.ExternalTodoApiOptions> options, ILogger<ExternalTodoClient> logger) : IExternalTodoClient
{
    public class ExternalTodoApiOptions
    {
        public string TodosUrl { get; set; } = string.Empty;
    }
    private readonly HttpClient _httpClient = httpClient;

    private readonly ExternalTodoApiOptions _options = options.Value;

    private readonly ILogger<ExternalTodoClient> _logger = logger;

    public async Task<IEnumerable<ExternalTodoListDto>> GetExternalTodoListsAsync(CancellationToken token)
    {
        try {
            var response = await _httpClient.GetAsync(_options.TodosUrl, token);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<ExternalTodoListDto>>(token)
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
