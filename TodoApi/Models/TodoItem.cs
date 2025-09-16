using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models;

public class TodoItem
{
    public int Id { get; set; }

    [MaxLength(100)]
    public required string Title { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [DefaultValue(false)]
    public bool IsCompleted { get; set; }

    public long TodoListId { get; set; }

    public TodoList TodoList { get; set; } = null!;
}
