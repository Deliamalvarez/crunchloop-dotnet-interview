
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.ExternalService;

public class TodoSyncService(IExternalTodoClient externalClient, IServiceProvider serviceProvider) : ITodoSyncService
{

    private readonly IExternalTodoClient _externalClient = externalClient;

    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task SyncronizeExternalTodoItemsAsync(CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TodoContext>();



        var externalTodos = await FetchExternalTodosAsync(token);
        if (!externalTodos.Any())
        {
            Console.WriteLine($"No external todos found to synchronize.");
            return;
        }

        foreach (var externalTodo in externalTodos)
        {
            var existingList = await context.TodoList
                .Include(t => t.Items)
                .FirstOrDefaultAsync(list => list.ExternalId == externalTodo.Id, token);


            if (existingList is not null && existingList.Items.All(item => externalTodo.Items.Any(ext => ext.Id == item.ExternalId))
            {
                Console.WriteLine($"List {existingList.Id} {existingList.Name} is already synchronized");
                continue;
            }
            else if (existingList is null)
            {
                existingList = new TodoList { Name = externalTodo.Name, ExternalId = externalTodo.Id };
                context.TodoList.Add(existingList);
                await context.SaveChangesAsync(token);
            }

            var existingExternalIds = await context.TodoItem
            .Where(item => item.TodoListId == existingList.Id && item.ExternalId != null)
            .Select(item => item.ExternalId)
            .ToListAsync(token);

            var missingTodos = externalTodo?.Items
                ?.Where(item => !existingExternalIds.Contains(item.Id));
            if (missingTodos is null || !missingTodos.Any())
            {
                Console.WriteLine($"External TodoList is already synchronized.");
                continue;
            }

            await AddTodosToDatabaseAsync(context, listId: existingList.Id, missingTodos, token);
        }

    }

    private async Task<IEnumerable<ExternalTodoListDto>> FetchExternalTodosAsync(CancellationToken token)
    {
        return await _externalClient.GetExternalTodoListsAsync(token);
    }

    private async Task AddTodosToDatabaseAsync(TodoContext context, long listId, IEnumerable<ExternalTodoItemDto> todos, CancellationToken token)
    {
        if (todos is null || !todos.Any()) return;

        var newTodos = todos.Select(todo => new TodoItem
        {
            Title = todo.Id,
            Description = todo.Description,
            IsCompleted = todo.Completed,
            TodoListId = listId,
            ExternalId = todo.Id
        });

        await context.TodoItem.AddRangeAsync(newTodos, token);

        await context.SaveChangesAsync(token);
    }
}
