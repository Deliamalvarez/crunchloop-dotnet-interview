using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using TodoApi.BackgroundServices;
using TodoApi.Extensions;
using TodoApi.ExternalService;
using static TodoApi.ExternalService.ExternalTodoClient;

var builder = WebApplication.CreateBuilder(args);
builder
    .Services.AddDbContext<TodoContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
    )
    .AddEndpointsApiExplorer()
    .AddControllers();

builder.Services.AddHttpClient<IExternalTodoClient, ExternalTodoClient>(client =>
{
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
})
.AddPolicyHandler(HttpClientPolicies.GetRetryPolicy())
.AddPolicyHandler(HttpClientPolicies.GetCircuitBreakerPolicy())
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10))); ;

builder.Services.Configure<ExternalTodoApiOptions>(
    builder.Configuration.GetSection("ExternalTodoApi"));
builder.Services.AddTransient<IExternalTodoClient, ExternalTodoClient>();
builder.Services.AddTransient<ITodoSyncService, TodoSyncService>();

builder.Services.AddHostedService<SyncTodoItemsWorker>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseAuthorization();
app.MapControllers();
app.Run();



