namespace QuizGenerationService.Application.DTOs;

public class ConceptRelationshipDto
{
    public string Id { get; set; } = string.Empty;
    public string SourceConceptId { get; set; } = string.Empty;
    public string TargetConceptId { get; set; } = string.Empty;
    public string SourceConceptName { get; set; } = string.Empty;
    public string TargetConceptName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Strength { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsStrongRelationship { get; set; }
    public bool IsBidirectional { get; set; }
    public bool IsHierarchical { get; set; }
} 