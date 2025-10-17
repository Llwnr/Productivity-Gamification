using Gamification.Core.Interfaces;
using Gamification.Core.Models;
using Polly;
using Polly.Retry;
using System.Threading.Channels;

namespace Gamification.WebAPI.Services;

public class AnalysisQueryManager : BackgroundService, IAnalysisQueryManager
{
    // BlockingCollection is thread-safe and handles waiting for items
    private List<Prompt> prompts = new  List<Prompt>();

    // Inject IServiceScopeFactory to resolve scoped services within the ExecuteAsync loop
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AnalysisQueryManager> _logger; // Use specific logger type

    // Constructor: Inject dependencies
    public AnalysisQueryManager(IServiceScopeFactory scopeFactory, ILogger<AnalysisQueryManager> logger){
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    // Public method to enqueue new analysis queries
    public async Task EnqueueAnalysisQuery(Prompt prompt){
        //When new prompt comes in, it means new site/tab visited. In that case, set the previous site/tab visited as visit ended.
        prompts.Add(prompt);
        _logger.LogInformation("Enqueued analysis query for prompt: {PromptKey}", prompt.Title);
    }

    // The core execution logic of the background service
    protected override async Task ExecuteAsync(CancellationToken stoppingToken){
        _logger.LogInformation("Are you even running");
        int batchInterval = 60*1000*7;
        int minimumPromptLimit = 5;
        while (!stoppingToken.IsCancellationRequested){
            await Task.Delay(batchInterval, stoppingToken);
            _logger.LogInformation("Performing scheduled analysis");
            if (prompts.Count < minimumPromptLimit){
                _logger.LogInformation($"Prompt count {prompts.Count} is less than " + minimumPromptLimit);
                continue;
            }
            _logger.LogInformation($"Prompt count:{prompts.Count}, starting analysis");
            // Define the Polly retry policy once
            AsyncRetryPolicy retryPolicy = DefineRetryPolicy();
            try{
                await retryPolicy.ExecuteAsync(async () => {
                    // _logger.LogInformation("Attempting analysis for prompt: {PromptKey}, User: {UserId}",
                    //     prompts[0].Title, prompts[0].UserId);
                    using (var scope = _scopeFactory.CreateScope()){
                        // Resolve ISiteAnalysisService from the current scope
                        var siteAnalysisService =
                            scope.ServiceProvider.GetRequiredService<ISiteAnalysisService>();
                        await siteAnalysisService.AnalyzeSites(prompts);
                    }
                    _logger.LogInformation("Finished analyzing the prompt. Clearing up prompt now.");
                    prompts.Clear();
                });
                
            }
            catch (OperationCanceledException){
                // This exception is expected when the stoppingToken is cancelled (e.g., application shutdown).
                // It allows for a graceful exit of the background service.
                _logger.LogInformation("AnalysisQueryManager background service is stopping gracefully.");
            }
            catch (Exception ex){
                // Catch any other unexpected exceptions that might occur outside the processing loop
                _logger.LogCritical(ex, "An unhandled exception occurred in AnalysisQueryManager background service.");
            }
        }
        // No need for 'return;' here. The method will naturally complete when the foreach loop exits.
    }

    // Helper method to define the Polly retry policy
    private AsyncRetryPolicy DefineRetryPolicy(){// Changed to private as it's an internal helper
        return Policy
            .Handle<Exception>() // Handle any exception. Be more specific if you know the types of transient errors.
            .WaitAndRetryAsync(
                4, // Retry up to 4 times
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(3, retryAttempt)), // Exponential back-off: 2s, 4s, 8s
                (exception, timeSpan, retryCount, context) => {
                    // Log a warning before each retry attempt
                    _logger.LogWarning(
                        exception,
                        "Analysis failed. Retrying attempt {RetryCount} in {TimeSpan} for current query.",
                        retryCount,
                        timeSpan);
                });
    }

    // Optional: Override StopAsync for cleanup if needed (e.g., signaling _queries.CompleteAdding())
    public override async Task StopAsync(CancellationToken cancellationToken){
        _logger.LogInformation("AnalysisQueryManager is signaling completion to the queue.");
        await base.StopAsync(cancellationToken);
    }
}