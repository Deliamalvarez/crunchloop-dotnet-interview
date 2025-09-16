namespace TodoApi.Dtos;

public record CreateTodoItem
{
    public required string Title { get; init; }
    public string Description { get; init; }

}
