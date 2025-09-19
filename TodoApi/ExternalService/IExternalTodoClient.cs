namespace TodoApi.ExternalService;

public interface IExternalTodoClient
{
    Task<IEnumerable<ExternalTodoListDto>> GetExternalTodoListsAsync(CancellationToken token);
}
