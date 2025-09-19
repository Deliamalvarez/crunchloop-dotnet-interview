using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TodoApi.Models;

public class TodoList
{
    public long Id { get; set; }
    public required string Name { get; set; }

    public string? ExternalId { get; set; }
    public ICollection<TodoItem> Items { get; set; } = new List<TodoItem>();

}
