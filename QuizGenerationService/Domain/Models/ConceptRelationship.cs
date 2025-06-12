using QuizGenerationService.Domain.ValueObjects;

namespace QuizGenerationService.Domain.Models;

public class ConceptRelationship
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SourceConceptId { get; set; } = string.Empty;
    public string TargetConceptId { get; set; } = string.Empty;
    public RelationshipType Type { get; set; } = RelationshipType.Related;
    public string Description { get; set; } = string.Empty;
    public double Strength { get; set; } = 0.5;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static ConceptRelationship Create(
        string sourceId, 
        string targetId, 
        RelationshipType type, 
        string description = "", 
        double strength = 0.5)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            throw new ArgumentException("Source concept ID cannot be empty", nameof(sourceId));
        
        if (string.IsNullOrWhiteSpace(targetId))
            throw new ArgumentException("Target concept ID cannot be empty", nameof(targetId));
        
        if (sourceId == targetId)
            throw new ArgumentException("Source and target concepts cannot be the same");
        
        if (strength < 0.0 || strength > 1.0)
            throw new ArgumentException("Strength must be between 0.0 and 1.0", nameof(strength));

        return new ConceptRelationship
        {
            SourceConceptId = sourceId,
            TargetConceptId = targetId,
            Type = type,
            Description = description,
            Strength = strength
        };
    }

    public bool IsStrongRelationship => Strength >= 0.7;
    public bool IsMediumRelationship => Strength >= 0.4 && Strength < 0.7;
    public bool IsWeakRelationship => Strength < 0.4;

    public bool IsBidirectional => Type == RelationshipType.Related || Type == RelationshipType.Similar || Type == RelationshipType.Opposite;
    public bool IsHierarchical => Type == RelationshipType.PartOf || Type == RelationshipType.InstanceOf || Type == RelationshipType.Influences;
} 