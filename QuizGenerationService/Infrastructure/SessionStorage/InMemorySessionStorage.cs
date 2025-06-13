using Microsoft.Extensions.Caching.Memory;
using QuizGenerationService.Domain.Interfaces;
using QuizGenerationService.Domain.Models;
using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Infrastructure.SessionStorage;

public class InMemorySessionStorage : ISessionStorage
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<InMemorySessionStorage> _logger;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(60);

    public InMemorySessionStorage(IMemoryCache cache, ILogger<InMemorySessionStorage> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<QuizSession?> GetSessionAsync(SessionId sessionId)
    {
        var cacheKey = GetCacheKey(sessionId);
        var session = _cache.Get<QuizSession>(cacheKey);
        
        if (session != null)
        {
            _logger.LogDebug("Retrieved session {SessionId} from memory cache", sessionId);
        }
        
        return Task.FromResult(session);
    }

    public Task SaveSessionAsync(QuizSession session)
    {
        var cacheKey = GetCacheKey(session.Id);
        
        var cacheOptions = new MemoryCacheEntryOptions
        {
            SlidingExpiration = _cacheExpiration,
            Priority = CacheItemPriority.Normal
        };

        _cache.Set(cacheKey, session, cacheOptions);
        
        _logger.LogDebug("Saved session {SessionId} to memory cache", session.Id);
        
        return Task.CompletedTask;
    }

    public Task<List<SessionSummary>> GetSessionSummariesAsync()
    {
        var summaries = new List<SessionSummary>();
        
        if (_cache is MemoryCache memoryCache)
        {
            var field = typeof(MemoryCache).GetField("_coherentState", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field?.GetValue(memoryCache) is IDictionary<object, object> coherentState)
            {
                foreach (var kvp in coherentState)
                {
                    if (kvp.Key.ToString()?.StartsWith("session_") == true && kvp.Value is QuizSession session)
                    {
                        summaries.Add(SessionSummary.FromSession(session));
                    }
                }
            }
        }

        summaries = summaries.OrderByDescending(s => s.LastModifiedAt).ToList();
        
        _logger.LogDebug("Retrieved {Count} session summaries from memory cache", summaries.Count);
        
        return Task.FromResult(summaries);
    }

    public Task DeleteSessionAsync(SessionId sessionId)
    {
        var cacheKey = GetCacheKey(sessionId);
        _cache.Remove(cacheKey);
        
        _logger.LogDebug("Deleted session {SessionId} from memory cache", sessionId);
        
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(SessionId sessionId)
    {
        var cacheKey = GetCacheKey(sessionId);
        var exists = _cache.TryGetValue(cacheKey, out _);
        
        return Task.FromResult(exists);
    }

    private static string GetCacheKey(SessionId sessionId) => $"session_{sessionId.Value}";
} 