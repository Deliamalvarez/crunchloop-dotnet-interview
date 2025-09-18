
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;
using TodoApi.Models;

namespace TodoApi.ExternalService;

public class TodoSyncService(IExternalTodoClient externalClient, IServiceProvider serviceProvider) : ITodoSyncService
{

    private readonly IExternalTodoClient _externalClient = externalClient;

    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task SyncronizeExternalTodoItemsAsync(CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<TodoContext>();
        var validListId = await GetTodoListByNameAsync(context, token);


        var externalTodos = await FetchExternalTodosAsync(token);


        var existingExternalIds = await context.TodoItem
            .Where(item => item.TodoListId == validListId && item.ExternalTodoId != null)
            .Select(item => item.ExternalTodoId)
            .ToListAsync(token);

        var missingTodos = externalTodos.Where(todo => !existingExternalIds.Contains(todo.Id)).ToList();

        if (!missingTodos.Any())
        {
            Console.WriteLine($"External TodoList is already synchronized.");
            return;
        }

        await AddTodosToDatabaseAsync(context, validListId, missingTodos, token);

    }

    private async Task<long> GetTodoListByNameAsync(TodoContext context, CancellationToken token)
    {
        var todoList = await context.TodoList
            .FirstOrDefaultAsync(listItem => listItem.Name == "external", token);
        if (todoList is null)
        {
            todoList = new TodoList
            {
                Name = "external"
            };
            context.TodoList.Add(todoList);
            await context.SaveChangesAsync(token);
        }
        return todoList.Id;
    }

    private async Task<IEnumerable<ExternalTodoDto>> FetchExternalTodosAsync(CancellationToken token)
    {
        return await _externalClient.GetTodosAsync(token);
    }

    private async Task AddTodosToDatabaseAsync(TodoContext context, long listId, IEnumerable<ExternalTodoDto> todos, CancellationToken token)
    {

        var newTodos = todos.Select(todo => new TodoItem
        {
            Title = todo.Title,
            Description = string.Empty,
            IsCompleted = todo.Completed,
            TodoListId = listId,
            ExternalTodoId = todo.Id
        });

        await context.TodoItem.AddRangeAsync(newTodos, token);

        await context.SaveChangesAsync(token);
    }
}
