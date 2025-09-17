namespace TodoApi.ExternalService;

public interface ITodoSyncService
{
    Task SyncronizeExternalTodoItemsAsync(CancellationToken token);
}
