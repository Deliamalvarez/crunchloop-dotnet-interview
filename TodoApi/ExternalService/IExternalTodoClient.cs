namespace TodoApi.ExternalService;

public interface IExternalTodoClient
{
    Task<IEnumerable<ExternalTodoDto>> GetTodosAsync(CancellationToken token);
}
