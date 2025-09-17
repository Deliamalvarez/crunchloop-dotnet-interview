using Microsoft.Extensions.Primitives;
using System.Text.Json.Serialization;

namespace TodoApi.Dtos;

public record TodoItemResponse([property: JsonPropertyName("id")] long Id, [property: JsonPropertyName("title")] string Title, [property: JsonPropertyName("description")] string? Description, [property: JsonPropertyName("isCompleted")] bool IsCompleted, [property: JsonPropertyName("externalId")] long? ExternalTodoId);
