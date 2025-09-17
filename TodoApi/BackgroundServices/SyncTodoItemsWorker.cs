using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TodoApi.ExternalService;

namespace TodoApi.BackgroundServices;

public class SyncTodoItemsWorker(ITodoSyncService todoSyncService, ILogger<SyncTodoItemsWorker> logger) : BackgroundService
{
    private readonly ITodoSyncService _todoSyncService = todoSyncService;
    private readonly ILogger<SyncTodoItemsWorker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await _todoSyncService.SyncronizeExternalTodoItemsAsync(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during todo sync");
            }

            // wait before next sync (avoid hammering external API)
            await Task.Delay(TimeSpan.FromMinutes(15), token);
        }
    }
}
