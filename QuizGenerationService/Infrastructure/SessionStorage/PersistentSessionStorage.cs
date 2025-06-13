using System.Text.Json;
using QuizGenerationService.Domain.Interfaces;
using QuizGenerationService.Domain.Models;
using QuizGenerationService.Domain.ValueObjects;
using QuizGenerationService.Infrastructure.JsonConverters;

namespace QuizGenerationService.Infrastructure.SessionStorage;

public class PersistentSessionStorage : ISessionStorage
{
    private readonly string _storageDirectory;
    private readonly ILogger<PersistentSessionStorage> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public PersistentSessionStorage(IConfiguration configuration, ILogger<PersistentSessionStorage> logger)
    {
        _logger = logger;
        _storageDirectory = configuration["SessionStorage:Directory"] ?? Path.Combine(Environment.CurrentDirectory, "sessions");
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters =
            {
                new SessionIdJsonConverter(),
                new QuestionIdJsonConverter(),
                new InterestRatingJsonConverter()
            }
        };

        EnsureStorageDirectoryExists();
    }

    public async Task<QuizSession?> GetSessionAsync(SessionId sessionId)
    {
        var filePath = GetSessionFilePath(sessionId);
        
        if (!File.Exists(filePath))
        {
            _logger.LogDebug("Session file not found: {FilePath}", filePath);
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var session = JsonSerializer.Deserialize<QuizSession>(json, _jsonOptions);
            
            _logger.LogDebug("Retrieved session {SessionId} from persistent storage", sessionId);
            
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize session {SessionId}", sessionId);
            return null;
        }
    }

    public async Task SaveSessionAsync(QuizSession session)
    {
        var filePath = GetSessionFilePath(session.Id);
        
        try
        {
            var json = JsonSerializer.Serialize(session, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
            
            _logger.LogDebug("Saved session {SessionId} to persistent storage", session.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save session {SessionId}", session.Id);
            throw;
        }
    }

    public async Task<List<SessionSummary>> GetSessionSummariesAsync()
    {
        var summaries = new List<SessionSummary>();
        
        try
        {
            var sessionFiles = Directory.GetFiles(_storageDirectory, "*.json");
            
            foreach (var filePath in sessionFiles)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    var session = JsonSerializer.Deserialize<QuizSession>(json, _jsonOptions);
                    
                    if (session != null)
                    {
                        summaries.Add(SessionSummary.FromSession(session));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load session from file: {FilePath}", filePath);
                }
            }
            
            summaries = summaries.OrderByDescending(s => s.LastModifiedAt).ToList();
            
            _logger.LogDebug("Retrieved {Count} session summaries from persistent storage", summaries.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve session summaries");
        }
        
        return summaries;
    }

    public Task DeleteSessionAsync(SessionId sessionId)
    {
        var filePath = GetSessionFilePath(sessionId);
        
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogDebug("Deleted session {SessionId} from persistent storage", sessionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete session {SessionId}", sessionId);
            throw;
        }
        
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(SessionId sessionId)
    {
        var filePath = GetSessionFilePath(sessionId);
        var exists = File.Exists(filePath);
        
        return Task.FromResult(exists);
    }

    private void EnsureStorageDirectoryExists()
    {
        if (!Directory.Exists(_storageDirectory))
        {
            Directory.CreateDirectory(_storageDirectory);
            _logger.LogInformation("Created session storage directory: {Directory}", _storageDirectory);
        }
    }

    private string GetSessionFilePath(SessionId sessionId)
    {
        return Path.Combine(_storageDirectory, $"{sessionId.Value}.json");
    }
} 