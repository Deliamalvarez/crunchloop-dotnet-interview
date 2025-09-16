namespace TodoApi.Dtos;

public record UpdateTodoItem
{
    public required string Title { get; init; }
    public string Description { get; init; }
}
