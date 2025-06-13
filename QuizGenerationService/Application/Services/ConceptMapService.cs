using QuizGenerationService.Application.DTOs;
using QuizGenerationService.Application.Interfaces;
using QuizGenerationService.Application.Validation;
using QuizGenerationService.Domain.Interfaces;
using QuizGenerationService.Domain.ValueObjects;
using QuizGenerationService.Infrastructure.ExternalServices;

namespace QuizGenerationService.Application.Services;

public class ConceptMapService : IConceptMapService
{
    private readonly ISessionStorage _sessionStorage;
    private readonly KeywordExtractionApiService _keywordExtractionService;
    private readonly ConceptMapApiService _conceptMapApiService;
    private readonly GenerateConceptMapValidator _validator;
    private readonly ILogger<ConceptMapService> _logger;

    public ConceptMapService(
        ISessionStorage sessionStorage,
        KeywordExtractionApiService keywordExtractionService,
        ConceptMapApiService conceptMapApiService,
        GenerateConceptMapValidator validator,
        ILogger<ConceptMapService> logger)
    {
        _sessionStorage = sessionStorage;
        _keywordExtractionService = keywordExtractionService;
        _conceptMapApiService = conceptMapApiService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ConceptMapDto> GenerateConceptMapAsync(GenerateConceptMapRequestDto request)
    {
        try
        {
            _logger.LogInformation("Starting concept map generation for session: {SessionId}, topic: {Topic}", 
                                 request.SessionId, request.Topic);

            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join("; ", validationResult.Errors);
                _logger.LogWarning("Validation failed for concept map request: {Errors}", errorMessage);
                throw new ArgumentException($"Invalid request: {errorMessage}");
            }

            var context = await GatherContextAsync(request);
            var focusKeywords = await GetFocusKeywordsAsync(request, context);
            
            var conceptMap = await _conceptMapApiService.GenerateConceptMapAsync(
                context.Topic,
                context.Language,
                focusKeywords,
                context.Book,
                context.SessionId,
                request.MaxNodes,
                request.IncludePhilosophers,
                request.IncludeHistoricalContext);

            var dto = MapToDto(conceptMap);
            
            _logger.LogInformation("Successfully generated concept map with {NodeCount} nodes and {RelationshipCount} relationships", 
                                 dto.TotalNodes, dto.TotalRelationships);

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate concept map for request");
            throw;
        }
    }

    private async Task<ConceptMapContext> GatherContextAsync(GenerateConceptMapRequestDto request)
    {
        var context = new ConceptMapContext
        {
            Topic = request.Topic ?? "Philosophy",
            Book = request.Book,
            Language = !string.IsNullOrWhiteSpace(request.Language) 
                ? Language.FromString(request.Language) 
                : Language.Polish
        };

        if (!string.IsNullOrWhiteSpace(request.SessionId))
        {
            try
            {
                var sessionId = SessionId.FromString(request.SessionId);
                var session = await _sessionStorage.GetSessionAsync(sessionId);
                
                if (session != null)
                {
                    context.SessionId = sessionId;
                    context.Topic = session.Topic;
                    context.Book = session.Book;
                    context.Language = session.Language;
                    context.Session = session;
                    
                    _logger.LogDebug("Enhanced context from session {SessionId}: topic={Topic}, language={Language}", 
                                   request.SessionId, context.Topic, context.Language.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load session context for {SessionId}, using provided context", request.SessionId);
            }
        }

        return context;
    }

    private async Task<List<string>> GetFocusKeywordsAsync(GenerateConceptMapRequestDto request, ConceptMapContext context)
    {
        var focusKeywords = new List<string>(request.FocusKeywords);

        if (request.UseSessionInterests && context.Session != null)
        {
            try
            {
                var keywordExtraction = await _keywordExtractionService.ExtractKeywordsFromSessionAsync(context.Session);
                
                var highPriorityKeywords = keywordExtraction.GetHighPriorityKeywords(10);
                var extractedKeywords = highPriorityKeywords
                    .Where(k => k.RelevanceScore >= request.MinImportanceThreshold)
                    .Select(k => k.Keyword)
                    .ToList();

                focusKeywords.AddRange(extractedKeywords);
                
                var highInterestKeywords = context.Session.GetHighInterestKeywords();
                focusKeywords.AddRange(highInterestKeywords);

                focusKeywords = focusKeywords.Distinct().ToList();
                
                _logger.LogDebug("Enhanced focus keywords with {ExtractedCount} extracted keywords and {InterestCount} interest keywords", 
                               extractedKeywords.Count, highInterestKeywords.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract keywords for concept map, using provided keywords only");
            }
        }

        return focusKeywords;
    }

    private ConceptMapDto MapToDto(Domain.Models.ConceptMap conceptMap)
    {
        var nodes = conceptMap.Nodes.Select(n => MapToNodeDto(n, conceptMap)).ToList();
        var relationships = conceptMap.Relationships.Select(r => MapToRelationshipDto(r, conceptMap)).ToList();

        var nodesByType = conceptMap.GetNodesByTypeGrouped()
            .ToDictionary(
                kvp => kvp.Key.Value, 
                kvp => kvp.Value.Select(n => MapToNodeDto(n, conceptMap)).ToList());

        var mostConnectedNodes = conceptMap.GetMostConnectedNodes(5)
            .Select(n => MapToNodeDto(n, conceptMap))
            .ToList();

        return new ConceptMapDto
        {
            Id = conceptMap.Id,
            Topic = conceptMap.Topic,
            Book = conceptMap.Book,
            SessionId = conceptMap.SessionId?.Value,
            Language = conceptMap.Language.Name,
            Nodes = nodes,
            Relationships = relationships,
            CreatedAt = conceptMap.CreatedAt,
            GenerationSource = conceptMap.GenerationSource,
            TotalNodes = conceptMap.TotalNodes,
            TotalRelationships = conceptMap.TotalRelationships,
            CoreConceptCount = conceptMap.CoreConceptCount,
            MostConnectedNodes = mostConnectedNodes,
            NodesByType = nodesByType
        };
    }

    private ConceptNodeDto MapToNodeDto(Domain.Models.ConceptNode node, Domain.Models.ConceptMap conceptMap)
    {
        return new ConceptNodeDto
        {
            Id = node.Id,
            Name = node.Name,
            Description = node.Description,
            Type = node.Type.Value,
            ImportanceScore = node.ImportanceScore,
            RelatedPhilosopher = node.RelatedPhilosopher,
            HistoricalPeriod = node.HistoricalPeriod,
            Keywords = node.Keywords,
            CreatedAt = node.CreatedAt,
            IsHighImportance = node.IsHighImportance,
            IsCorePhilosophicalConcept = node.IsCorePhilosophicalConcept,
            ConnectivityCount = conceptMap.GetNodeConnectivity(node.Id)
        };
    }

    private ConceptRelationshipDto MapToRelationshipDto(Domain.Models.ConceptRelationship relationship, Domain.Models.ConceptMap conceptMap)
    {
        var sourceNode = conceptMap.GetNode(relationship.SourceConceptId);
        var targetNode = conceptMap.GetNode(relationship.TargetConceptId);

        return new ConceptRelationshipDto
        {
            Id = relationship.Id,
            SourceConceptId = relationship.SourceConceptId,
            TargetConceptId = relationship.TargetConceptId,
            SourceConceptName = sourceNode?.Name ?? "Unknown",
            TargetConceptName = targetNode?.Name ?? "Unknown",
            Type = relationship.Type.Value,
            Description = relationship.Description,
            Strength = relationship.Strength,
            CreatedAt = relationship.CreatedAt,
            IsStrongRelationship = relationship.IsStrongRelationship,
            IsBidirectional = relationship.IsBidirectional,
            IsHierarchical = relationship.IsHierarchical
        };
    }

    private class ConceptMapContext
    {
        public string Topic { get; set; } = string.Empty;
        public string? Book { get; set; }
        public Language Language { get; set; } = Language.Polish;
        public SessionId? SessionId { get; set; }
        public Domain.Models.QuizSession? Session { get; set; }
    }
} 