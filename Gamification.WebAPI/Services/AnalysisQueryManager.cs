using Gamification.Core.Interfaces;
using Gamification.Core.Models;
using Polly;
using Polly.Retry;
using System.Threading.Channels;

namespace Gamification.WebAPI.Services;

public class AnalysisQueryManager : BackgroundService, IAnalysisQueryManager
{
    // BlockingCollection is thread-safe and handles waiting for items
    private readonly Channel<KeyValuePair<Prompt, string>> _queriesChannel =
        Channel.CreateUnbounded<KeyValuePair<Prompt, string>>();

    // Inject IServiceScopeFactory to resolve scoped services within the ExecuteAsync loop
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AnalysisQueryManager> _logger; // Use specific logger type

    // Constructor: Inject dependencies
    public AnalysisQueryManager(IServiceScopeFactory scopeFactory, ILogger<AnalysisQueryManager> logger){
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    // Public method to enqueue new analysis queries
    public async Task EnqueueAnalysisQuery(Prompt prompt, string userId){
        await _queriesChannel.Writer.WriteAsync(new KeyValuePair<Prompt, string>(prompt, userId));
        _logger.LogInformation("Enqueued analysis query for prompt: {PromptKey}", prompt.Title);
    }

    // The core execution logic of the background service
    protected override async Task ExecuteAsync(CancellationToken stoppingToken){
        while (!stoppingToken.IsCancellationRequested){
            // Define the Polly retry policy once
            AsyncRetryPolicy retryPolicy = DefineRetryPolicy();
            try{
                await foreach (var query in _queriesChannel.Reader.ReadAllAsync(stoppingToken)){
                    //Record the exact time the user visited the site.
                    DateTime visitTime =  DateTime.UtcNow;
                    try{
                        // Execute the analysis with the defined retry policy
                        await retryPolicy.ExecuteAsync(async () => {
                            _logger.LogInformation("Attempting analysis for prompt: {PromptKey}, User: {UserId}",
                                query.Key.Title, query.Value);

                            // Create a new scope for each operation to correctly handle scoped services
                            using (var scope = _scopeFactory.CreateScope()){
                                // Resolve ISiteAnalysisService from the current scope
                                var siteAnalysisService =
                                    scope.ServiceProvider.GetRequiredService<ISiteAnalysisService>();
                                await siteAnalysisService.AnalyzeSite(query.Key, query.Value, visitTime);
                            }

                            _logger.LogInformation("Successfully analyzed site for prompt: {PromptKey}",
                                query.Key.Title);
                        });
                    }
                    catch (Exception ex){
                        // This catch block is hit if Polly's retry policy fails all attempts.
                        // Log the final failure and potentially handle the "poison message" (e.g., move to dead-letter queue).
                        _logger.LogError(ex,
                            "The operation failed after all retries for prompt: {PromptKey}, User: {UserId}. Query will not be reprocessed.",
                            query.Key.Title, query.Value);
                    }
                }
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
                3, // Retry up to 3 times
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