namespace QuizGenerationService.Application.DTOs;

public class ConceptMapDto
{
    public string Id { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string? Book { get; set; }
    public string? SessionId { get; set; }
    public string Language { get; set; } = string.Empty;
    public List<ConceptNodeDto> Nodes { get; set; } = new();
    public List<ConceptRelationshipDto> Relationships { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string? GenerationSource { get; set; }
    
    public int TotalNodes { get; set; }
    public int TotalRelationships { get; set; }
    public int CoreConceptCount { get; set; }
    public List<ConceptNodeDto> MostConnectedNodes { get; set; } = new();
    public Dictionary<string, List<ConceptNodeDto>> NodesByType { get; set; } = new();
} 