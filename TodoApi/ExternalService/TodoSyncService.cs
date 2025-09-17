
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
        var validListIds = await GetValidTodoListIdsAsync(context, token);

        if (!validListIds.Any())
        {
            Console.WriteLine("There are no list items now in the system, aborting sync operation");
            return;
        }

        var externalTodos = await FetchExternalTodosAsync(token);

        foreach (var listId in validListIds)
        {
            var existingExternalIds = await context.TodoItem
                .Where(item => item.TodoListId == listId && item.ExternalTodoId != null)
                .Select(item => item.ExternalTodoId)
                .ToListAsync(token);

            var missingTodos = externalTodos.Where(todo => !existingExternalIds.Contains(todo.Id)).ToList();

            if (!missingTodos.Any())
            {
                Console.WriteLine($"TodoList {listId} is already synchronized.");
                continue;
            }

            await AddTodosToDatabaseAsync(context, listId, missingTodos, token);
        }
    }


    private async Task<IEnumerable<long>> GetValidTodoListIdsAsync(TodoContext context, CancellationToken token)
    {
        return await context.TodoList
            .Select(list => list.Id)
            .ToListAsync(token);
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
